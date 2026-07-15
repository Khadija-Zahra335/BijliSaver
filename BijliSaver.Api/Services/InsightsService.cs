// The "brain": slab position, tax breakdown, appliance translation,
// next-month prediction, savings tip. Pure math — no AI here.
//
// Key product rules (from Sprint 0, validated against real bills):
// - Tax percentage is computed against CURRENT BILL, never payable.
//   (A Rs 64k-arrears bill would otherwise make taxes look tiny.)
// - Estimated ("EX") history months are excluded from prediction math.
// - Slab logic: crossing a threshold reprices ALL units, hence the
//   "units to next slab" warning is the app's most valuable output.

using BijliSaver.Api.Data;
using BijliSaver.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BijliSaver.Api.Services;

public class InsightsService(AppDbContext db)
{
    public async Task<BillInsight> ComputeAsync(Bill bill, CancellationToken ct = default)
    {
        var insight = new BillInsight { BillId = bill.Id, ComputedAt = DateTime.UtcNow };

        // ---- Tax breakdown (categories from charge_definitions) ----
        var defsById = await db.ChargeDefinitions.ToDictionaryAsync(d => d.Id, ct);
        decimal taxTotal = 0, energyTotal = 0;
        foreach (var charge in bill.Charges)
        {
            if (charge.ChargeDefId is null) continue;
            var category = defsById[charge.ChargeDefId.Value].Category;
            if (category is "tax" or "surcharge" or "fee") taxTotal += charge.Amount;
            else if (category is "energy") energyTotal += charge.Amount;
        }
        insight.TaxTotal = Math.Round(taxTotal, 2);
        insight.EnergyTotal = Math.Round(energyTotal, 2);
        if (bill.CurrentBill is > 0)
            insight.TaxPercentage = Math.Round(taxTotal / bill.CurrentBill.Value * 100, 1);

        insight.HasArrears = (bill.Arrears ?? 0) > 0;

        // ---- Slab analysis ----
        decimal? currentRate = null;
        if (bill.UnitsConsumed is int units)
        {
            var slabs = await GetSlabsForBillAsync(bill, ct);
            if (slabs.Count > 0)
            {
                var current = slabs.FirstOrDefault(s =>
                    units >= s.SlabMinUnits && (s.SlabMaxUnits is null || units <= s.SlabMaxUnits));
                var next = current?.SlabMaxUnits is int max
                    ? slabs.FirstOrDefault(s => s.SlabMinUnits == max + 1)
                    : null;

                currentRate = current?.RatePerUnit;
                insight.SlabReached = current?.SlabMaxUnits ?? current?.SlabMinUnits;
                if (current?.SlabMaxUnits is int ceiling && next is not null)
                {
                    insight.UnitsToNextSlab = ceiling - units + 1;
                    // Crossing reprices ALL units at the higher rate:
                    insight.NextSlabPenalty = Math.Round(
                        (next.RatePerUnit - current.RatePerUnit) * (ceiling + 1), 0);
                }
            }

            insight.ApplianceSummary = ApplianceSummary(units);
        }

        // ---- Prediction from the bill's own 12-month history ----
        var usable = bill.HistoryEntries
            .Where(h => !h.IsEstimated && h.Units is not null)
            .OrderByDescending(h => h.Month)
            .Take(3)
            .ToList();

        if (usable.Count >= 2 && bill.GopRate is decimal rate)
        {
            var avgUnits = (decimal)usable.Average(h => h.Units!.Value);
            // Rough forecast: avg units x current effective rate x ~1.30 for taxes.
            // Honest simplification for MVP; refine with per-charge math in v2.
            insight.PredictedNextAmt = Math.Round(avgUnits * rate * 1.30m, 0);
            insight.PredictionBasis = "history_table";
        }
        else
        {
            insight.PredictionBasis = "insufficient";
        }

        // ---- Savings tip ----
        insight.SavingsTip = BuildSavingsTip(insight, bill.UnitsConsumed, currentRate);

        return insight;
    }

    private async Task<List<TariffSlab>> GetSlabsForBillAsync(Bill bill, CancellationToken ct)
    {
        // Latest slab set effective on or before the billing month.
        var effective = await db.TariffSlabs
            .Where(s => s.DiscoId == bill.DiscoId && s.EffectiveFrom <= bill.BillingMonth)
            .MaxAsync(s => (DateOnly?)s.EffectiveFrom, ct);

        if (effective is null) return [];

        return await db.TariffSlabs
            .Where(s => s.DiscoId == bill.DiscoId && s.EffectiveFrom == effective)
            .OrderBy(s => s.SlabMinUnits)
            .ToListAsync(ct);
    }

    // kWh → everyday appliance terms. Approximate by design; the point is
    // intuition, not precision. 1.5-ton inverter AC ≈ 1.2 kWh per hour.
    private const double AcKwhPerHour = 1.2;
    private const double FanKwhPerHour = 0.08;

    private static string ApplianceSummary(int units)
    {
        var acHoursPerDay = Math.Round(units / 30.0 / AcKwhPerHour, 1);
        if (acHoursPerDay >= 1)
            return $"Your usage is about the same as running 1 air conditioner " +
                   $"for {acHoursPerDay:0.#} hours every day.";

        var fanHoursPerDay = Math.Round(units / 30.0 / FanKwhPerHour, 0);
        return $"Your usage is about the same as running a ceiling fan " +
               $"for {fanHoursPerDay:0} hours every day.";
    }

    // Concrete "cut this appliance, save this much" line — used as a fallback
    // when the AI advice call (richer, multi-tip) fails or is unavailable.
    private static string ApplianceSavingLine(int? unitsConsumed, decimal? ratePerUnit)
    {
        if (unitsConsumed is not int units || ratePerUnit is not decimal rate) return "";

        var acHoursPerDay = units / 30.0 / AcKwhPerHour;
        if (acHoursPerDay >= 1)
        {
            var monthlySaving = Math.Round((decimal)AcKwhPerHour * rate * 30, 0);
            return $" Cutting your air conditioner use by just 1 hour a day (about " +
                   $"{AcKwhPerHour:0.#} units) saves roughly Rs {monthlySaving:N0} a month.";
        }

        var fanSaving = Math.Round((decimal)FanKwhPerHour * rate * 30, 0);
        return $" Running your ceiling fan 1 hour less a day saves roughly Rs {fanSaving:N0} a month.";
    }

    private static string BuildSavingsTip(BillInsight insight, int? unitsConsumed, decimal? ratePerUnit)
    {
        var applianceLine = ApplianceSavingLine(unitsConsumed, ratePerUnit);

        if (insight.UnitsToNextSlab is int toNext and <= 50 && insight.NextSlabPenalty is decimal penalty)
            return $"Warning: you are only {toNext} units away from the next price level. " +
                   $"Crossing it makes your whole bill about Rs {penalty:N0} more expensive. " +
                   $"Try to stay under the limit this month.{applianceLine}";

        if (insight.UnitsToNextSlab is int room)
            return $"You have room to spare: {room} units before the next price level. " +
                   $"Keep your usage steady and your rate stays the same.{applianceLine}";

        return ("You are in the highest usage level. Reducing units always saves money " +
               $"at the top rate — every unit cut counts the most here.{applianceLine}").Trim();
    }
}
