// User-aware bill endpoints:
//   POST /api/bills/upload — works logged-in (bill belongs to you) OR anonymous
//   GET  /api/bills         — requires login; returns ONLY your bills
//   GET  /api/bills/{id}    — owner or anonymous-bill only
//   DELETE /api/bills/{id}  — owner only (anonymous bills undeletable via API)

using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using BijliSaver.Api.Data;
using BijliSaver.Api.Dtos;
using BijliSaver.Api.Models;
using BijliSaver.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BijliSaver.Api.Controllers;

[ApiController]
[Route("api/bills")]
public class BillsController(
    AppDbContext db,
    OcrClient ocr,
    InsightsService insights,
    ILogger<BillsController> logger) : ControllerBase
{
    private const long MaxUploadBytes = 10 * 1024 * 1024;
    private static readonly string[] AllowedTypes =
        ["image/jpeg", "image/png", "image/webp", "application/pdf"];

    private Guid? CurrentUserId =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    [HttpPost("upload")]
    [RequestSizeLimit(MaxUploadBytes)]
    public async Task<ActionResult<BillDetailDto>> Upload(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0) return BadRequest("No file uploaded.");
        if (!AllowedTypes.Contains(file.ContentType))
            return BadRequest($"Unsupported file type: {file.ContentType}");

        await using var stream = file.OpenReadStream();
        var ocrResult = await ocr.ExtractAsync(stream, file.FileName, file.ContentType, ct);

        if (ocrResult.Status == "not_a_bill")
            return UnprocessableEntity("This image does not look like an electricity bill. Please upload a photo of your LESCO bill.");
        if (ocrResult.Status == "failed" || ocrResult.Data is null)
            return UnprocessableEntity("We could not read this bill. Please try a clearer photo.");

        var data = ocrResult.Data;
        var disco = await db.Discos.SingleAsync(d => d.Code == data.Disco, ct);
        var userId = CurrentUserId;

        var bill = new Bill
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DiscoId = disco.Id,
            ReferenceNo = data.ReferenceNo,
            TariffCode = data.TariffCode,
            BillingMonth = ParseMonth(data.BillingMonth) ?? DateOnly.FromDateTime(DateTime.UtcNow),
            UnitsConsumed = data.UnitsConsumed,
            CurrentBill = data.CurrentBill,
            TotalFpa = data.TotalFpa,
            Arrears = data.Arrears ?? 0,
            PayableWithinDue = data.PayableWithinDue,
            PayableAfterDue = data.PayableAfterDue,
            LpSurcharge = data.LpSurcharge,
            FpaRate = data.FpaRate,
            FpaMonth = ParseMonth(data.FpaMonth),
            GopRate = data.GopRate,
            ReadingIsEstimated = data.ReadingIsEstimated,
            DueDate = ParseDate(data.DueDate),
            OcrStatus = ocrResult.Status,
            OcrConfidence = ocrResult.EffectiveConfidence,
            OcrRaw = JsonSerializer.Serialize(data),
            CreatedAt = DateTime.UtcNow,
        };

        // Duplicate check scoped to THIS user (different users may upload the same bill)
        var duplicate = await db.Bills.AnyAsync(b =>
            b.UserId == userId &&
            b.ReferenceNo == bill.ReferenceNo &&
            b.BillingMonth == bill.BillingMonth, ct);
        if (duplicate)
            return Conflict("This bill (same reference number and month) is already in your account.");

        var defsByCode = await db.ChargeDefinitions.ToDictionaryAsync(d => d.Code, ct);
        foreach (var line in data.Charges)
        {
            var code = ChargeLabelMapper.MapToCode(line.RawLabel);
            if (code == "SKIP") continue;
            if (code is null)
                logger.LogWarning("Unrecognized charge label: '{Label}' — stored as Other", line.RawLabel);

            bill.Charges.Add(new BillCharge
            {
                BillId = bill.Id,
                RawLabel = line.RawLabel,
                Amount = line.Amount,
                ChargeDefId = code is not null && defsByCode.TryGetValue(code, out var def)
                    ? def.Id : null,
            });
        }

        foreach (var h in data.History)
        {
            var month = ParseMonth(h.Month);
            if (month is null) continue;
            bill.HistoryEntries.Add(new BillHistoryEntry
            {
                BillId = bill.Id,
                Month = month.Value,
                Units = h.Units,
                Amount = h.Amount,
                IsEstimated = h.Estimated,
            });
        }

        // Compute insights even on "needs_review" — the extraction still has
        // usable data, and the warning banner already tells the user the
        // numbers might be imperfect. Withholding the savings plan entirely
        // was worse than showing a caveated one.
        if (ocrResult.Status is "done" or "needs_review")
        {
            bill.Insight = await insights.ComputeAsync(bill, ct);

            var month = bill.BillingMonth.Month;
            var advice = await ocr.AdviseAsync(new
            {
                billing_month = bill.BillingMonth.ToString("yyyy-MM"),
                units_consumed = bill.UnitsConsumed,
                current_bill = bill.CurrentBill,
                tax_total = bill.Insight.TaxTotal,
                tax_percentage = bill.Insight.TaxPercentage,
                slab_ceiling = bill.Insight.SlabReached,
                units_to_next_slab = bill.Insight.UnitsToNextSlab,
                next_slab_penalty = bill.Insight.NextSlabPenalty,
                avg_recent_units = bill.HistoryEntries
                    .Where(h => !h.IsEstimated && h.Units != null)
                    .OrderByDescending(h => h.Month).Take(3)
                    .Select(h => (double?)h.Units).DefaultIfEmpty(null).Average(),
                trend = TrendFromHistory(bill),
                season_hint = month >= 4 && month <= 9 ? "summer" : "winter",
                has_arrears = (bill.Arrears ?? 0) > 0,
                fpa_is_negative = (bill.TotalFpa ?? 0) < 0,
            }, ct);

            if (advice is not null)
                bill.Insight.SavingsTip =
                    advice.Summary + "\n\n" + string.Join("\n", advice.Tips.Select(t => "• " + t));
        }

        db.Bills.Add(bill);
        await db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = bill.Id }, await ToDetailDto(bill, ct));
    }

    [Authorize]
    [HttpGet]
    public async Task<List<BillSummaryDto>> List(CancellationToken ct) =>
        await db.Bills
            .Where(b => b.UserId == CurrentUserId)
            .OrderByDescending(b => b.BillingMonth)
            .Select(b => new BillSummaryDto(
                b.Id,
                b.BillingMonth.ToString("yyyy-MM"),
                b.UnitsConsumed,
                b.CurrentBill,
                b.PayableWithinDue,
                b.OcrStatus))
            .ToListAsync(ct);

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BillDetailDto>> GetById(Guid id, CancellationToken ct)
    {
        var bill = await db.Bills
            .Include(b => b.Charges).ThenInclude(c => c.ChargeDef)
            .Include(b => b.Insight)
            .SingleOrDefaultAsync(b => b.Id == id, ct);

        if (bill is null) return NotFound();
        // Owners see their bills; anonymous bills stay reachable by direct link.
        if (bill.UserId is not null && bill.UserId != CurrentUserId) return NotFound();

        return await ToDetailDto(bill, ct);
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var bill = await db.Bills.FindAsync([id], ct);
        if (bill is null || bill.UserId != CurrentUserId) return NotFound();
        db.Bills.Remove(bill);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    private async Task<BillDetailDto> ToDetailDto(Bill bill, CancellationToken ct)
    {
        foreach (var c in bill.Charges.Where(c => c.ChargeDefId != null && c.ChargeDef == null))
            c.ChargeDef = await db.ChargeDefinitions.FindAsync([c.ChargeDefId], ct);

        var charges = bill.Charges
            .OrderBy(c => c.ChargeDef?.SortOrder ?? 999)
            .Select(c => new ChargeLineDto(
                c.ChargeDef?.DisplayName ?? c.RawLabel,
                c.Amount,
                c.ChargeDef?.ExplanationEn ?? "An additional charge printed on your bill.",
                c.ChargeDef?.Category ?? "other"))
            .ToList();

        InsightDto? insightDto = null;
        if (bill.Insight is { } i)
        {
            string? arrearsNote = null;
            if (i.HasArrears && bill.CurrentBill is decimal current && bill.PayableWithinDue is decimal payable)
                arrearsNote = $"Your bill this month is Rs {current:N0}. The remaining " +
                              $"Rs {payable - current:N0} is previous unpaid bills — not new usage or taxes.";

            insightDto = new InsightDto(
                i.SlabReached, i.UnitsToNextSlab, i.NextSlabPenalty,
                i.TaxTotal, i.TaxPercentage, i.HasArrears, arrearsNote,
                i.ApplianceSummary, i.PredictedNextAmt, i.SavingsTip);
        }

        return new BillDetailDto(
            bill.Id,
            bill.BillingMonth.ToString("yyyy-MM"),
            bill.UnitsConsumed,
            bill.CurrentBill,
            bill.Arrears,
            bill.TotalFpa,
            bill.PayableWithinDue,
            bill.PayableAfterDue,
            bill.DueDate?.ToString("yyyy-MM-dd"),
            bill.OcrStatus,
            charges,
            insightDto);
    }

    private static string TrendFromHistory(Bill bill)
    {
        var months = bill.HistoryEntries
            .Where(h => !h.IsEstimated && h.Units != null)
            .OrderByDescending(h => h.Month).Take(6).ToList();
        if (months.Count < 4) return "stable";
        var recent = months.Take(3).Average(h => h.Units!.Value);
        var older = months.Skip(3).Average(h => h.Units!.Value);
        return recent > older * 1.15 ? "rising" : recent < older * 0.85 ? "falling" : "stable";
    }

    private static DateOnly? ParseMonth(string? yyyyMm) =>
        DateOnly.TryParseExact($"{yyyyMm}-01", "yyyy-MM-dd", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var d) ? d : null;

    private static DateOnly? ParseDate(string? yyyyMmDd) =>
        DateOnly.TryParseExact(yyyyMmDd, "yyyy-MM-dd", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var d) ? d : null;
}
