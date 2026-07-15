using BijliSaver.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BijliSaver.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Disco> Discos => Set<Disco>();
    public DbSet<TariffSlab> TariffSlabs => Set<TariffSlab>();
    public DbSet<ChargeDefinition> ChargeDefinitions => Set<ChargeDefinition>();
    public DbSet<Bill> Bills => Set<Bill>();
    public DbSet<User> Users => Set<User>();
    public DbSet<BillCharge> BillCharges => Set<BillCharge>();
    public DbSet<BillHistoryEntry> BillHistoryEntries => Set<BillHistoryEntry>();
    public DbSet<BillInsight> BillInsights => Set<BillInsight>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // Map to the snake_case tables created by schema_v2.sql
        mb.Entity<Disco>(e =>
        {
            e.ToTable("discos");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Code).HasColumnName("code");
            e.Property(x => x.Name).HasColumnName("name");
            e.Property(x => x.IsActive).HasColumnName("is_active");
        });

        mb.Entity<TariffSlab>(e =>
        {
            e.ToTable("tariff_slabs");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.DiscoId).HasColumnName("disco_id");
            e.Property(x => x.TariffCode).HasColumnName("tariff_code");
            e.Property(x => x.EffectiveFrom).HasColumnName("effective_from");
            e.Property(x => x.SlabMinUnits).HasColumnName("slab_min_units");
            e.Property(x => x.SlabMaxUnits).HasColumnName("slab_max_units");
            e.Property(x => x.RatePerUnit).HasColumnName("rate_per_unit");
            e.Property(x => x.IsProtected).HasColumnName("is_protected");
        });

        mb.Entity<ChargeDefinition>(e =>
        {
            e.ToTable("charge_definitions");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Code).HasColumnName("code");
            e.Property(x => x.DisplayName).HasColumnName("display_name");
            e.Property(x => x.ExplanationEn).HasColumnName("explanation_en");
            e.Property(x => x.ExplanationUr).HasColumnName("explanation_ur");
            e.Property(x => x.Category).HasColumnName("category");
            e.Property(x => x.CanBeNegative).HasColumnName("can_be_negative");
            e.Property(x => x.SortOrder).HasColumnName("sort_order");
        });

        mb.Entity<Bill>(e =>
        {
            e.ToTable("bills");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.DiscoId).HasColumnName("disco_id");
            e.Property(x => x.ReferenceNo).HasColumnName("reference_no");
            e.Property(x => x.TariffCode).HasColumnName("tariff_code");
            e.Property(x => x.BillingMonth).HasColumnName("billing_month");
            e.Property(x => x.UnitsConsumed).HasColumnName("units_consumed");
            e.Property(x => x.CurrentBill).HasColumnName("current_bill");
            e.Property(x => x.TotalFpa).HasColumnName("total_fpa");
            e.Property(x => x.Arrears).HasColumnName("arrears");
            e.Property(x => x.PayableWithinDue).HasColumnName("payable_within_due");
            e.Property(x => x.PayableAfterDue).HasColumnName("payable_after_due");
            e.Property(x => x.LpSurcharge).HasColumnName("lp_surcharge");
            e.Property(x => x.FpaRate).HasColumnName("fpa_rate");
            e.Property(x => x.FpaMonth).HasColumnName("fpa_month");
            e.Property(x => x.GopRate).HasColumnName("gop_rate");
            e.Property(x => x.ReadingIsEstimated).HasColumnName("reading_is_estimated");
            e.Property(x => x.DueDate).HasColumnName("due_date");
            e.Property(x => x.ImageUrl).HasColumnName("image_url");
            e.Property(x => x.OcrStatus).HasColumnName("ocr_status");
            e.Property(x => x.OcrConfidence).HasColumnName("ocr_confidence");
            e.Property(x => x.OcrRaw).HasColumnName("ocr_raw").HasColumnType("jsonb");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");

            e.HasMany(x => x.Charges).WithOne().HasForeignKey(c => c.BillId);
            e.HasMany(x => x.HistoryEntries).WithOne().HasForeignKey(h => h.BillId);
            e.HasOne(x => x.Insight).WithOne().HasForeignKey<BillInsight>(i => i.BillId);
        });

        mb.Entity<BillCharge>(e =>
        {
            e.ToTable("bill_charges");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.BillId).HasColumnName("bill_id");
            e.Property(x => x.ChargeDefId).HasColumnName("charge_def_id");
            e.Property(x => x.RawLabel).HasColumnName("raw_label");
            e.Property(x => x.Amount).HasColumnName("amount");
            e.HasOne(x => x.ChargeDef).WithMany().HasForeignKey(x => x.ChargeDefId);
        });

        mb.Entity<BillHistoryEntry>(e =>
        {
            e.ToTable("bill_history_entries");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.BillId).HasColumnName("bill_id");
            e.Property(x => x.Month).HasColumnName("month");
            e.Property(x => x.Units).HasColumnName("units");
            e.Property(x => x.Amount).HasColumnName("amount");
            e.Property(x => x.IsEstimated).HasColumnName("is_estimated");
        });

        mb.Entity<BillInsight>(e =>
        {
            e.ToTable("bill_insights");
            e.HasKey(x => x.BillId);
            e.Property(x => x.BillId).HasColumnName("bill_id");
            e.Property(x => x.SlabReached).HasColumnName("slab_reached");
            e.Property(x => x.UnitsToNextSlab).HasColumnName("units_to_next_slab");
            e.Property(x => x.NextSlabPenalty).HasColumnName("next_slab_penalty");
            e.Property(x => x.TaxTotal).HasColumnName("tax_total");
            e.Property(x => x.TaxPercentage).HasColumnName("tax_percentage");
            e.Property(x => x.EnergyTotal).HasColumnName("energy_total");
            e.Property(x => x.HasArrears).HasColumnName("has_arrears");
            e.Property(x => x.ApplianceSummary).HasColumnName("appliance_summary");
            e.Property(x => x.PredictedNextAmt).HasColumnName("predicted_next_amt");
            e.Property(x => x.PredictionBasis).HasColumnName("prediction_basis");
            e.Property(x => x.SavingsTip).HasColumnName("savings_tip");
            e.Property(x => x.ComputedAt).HasColumnName("computed_at");
        });

	mb.Entity<User>(e =>
        {
            e.ToTable("users");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Email).HasColumnName("email");
            e.Property(x => x.PasswordHash).HasColumnName("password_hash");
            e.Property(x => x.DisplayName).HasColumnName("display_name");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
        });
    }
}
