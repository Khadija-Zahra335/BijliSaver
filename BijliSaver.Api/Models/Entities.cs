// Entity classes mapping to the tables created by schema_v2.sql.
// We run the SQL script manually (pgAdmin/psql) and point EF Core at the
// existing tables — no migrations needed for the MVP.

namespace BijliSaver.Api.Models;

public class Disco
{
    public short Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public bool IsActive { get; set; } = true;
}

public class TariffSlab
{
    public int Id { get; set; }
    public short DiscoId { get; set; }
    public string TariffCode { get; set; } = "A-1";
    public DateOnly EffectiveFrom { get; set; }
    public int SlabMinUnits { get; set; }
    public int? SlabMaxUnits { get; set; }          // null = top slab
    public decimal RatePerUnit { get; set; }
    public bool IsProtected { get; set; }
}

public class ChargeDefinition
{
    public short Id { get; set; }
    public string Code { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string ExplanationEn { get; set; } = "";
    public string? ExplanationUr { get; set; }
    public string Category { get; set; } = "";      // energy|tax|surcharge|fee|adjustment|penalty
    public bool CanBeNegative { get; set; }
    public short SortOrder { get; set; }
}

public class Bill
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }               // nullable: anonymous MVP
    public short DiscoId { get; set; }
    public string? ReferenceNo { get; set; }
    public string? TariffCode { get; set; }
    public DateOnly BillingMonth { get; set; }
    public int? UnitsConsumed { get; set; }

    public decimal? CurrentBill { get; set; }
    public decimal? TotalFpa { get; set; }
    public decimal? Arrears { get; set; }
    public decimal? PayableWithinDue { get; set; }
    public decimal? PayableAfterDue { get; set; }
    public decimal? LpSurcharge { get; set; }

    public decimal? FpaRate { get; set; }
    public DateOnly? FpaMonth { get; set; }
    public decimal? GopRate { get; set; }
    public bool ReadingIsEstimated { get; set; }

    public DateOnly? DueDate { get; set; }
    public string? ImageUrl { get; set; }
    public string OcrStatus { get; set; } = "pending";
    public decimal? OcrConfidence { get; set; }
    public string? OcrRaw { get; set; }             // jsonb
    public DateTime CreatedAt { get; set; }

    public List<BillCharge> Charges { get; set; } = [];
    public List<BillHistoryEntry> HistoryEntries { get; set; } = [];
    public BillInsight? Insight { get; set; }
}

public class BillCharge
{
    public long Id { get; set; }
    public Guid BillId { get; set; }
    public short? ChargeDefId { get; set; }         // null = unrecognized label
    public string RawLabel { get; set; } = "";
    public decimal Amount { get; set; }

    public ChargeDefinition? ChargeDef { get; set; }
}

public class BillHistoryEntry
{
    public long Id { get; set; }
    public Guid BillId { get; set; }
    public DateOnly Month { get; set; }
    public int? Units { get; set; }
    public decimal? Amount { get; set; }
    public bool IsEstimated { get; set; }
}

public class BillInsight
{
    public Guid BillId { get; set; }
    public int? SlabReached { get; set; }
    public int? UnitsToNextSlab { get; set; }
    public decimal? NextSlabPenalty { get; set; }
    public decimal? TaxTotal { get; set; }
    public decimal? TaxPercentage { get; set; }
    public decimal? EnergyTotal { get; set; }
    public bool HasArrears { get; set; }
    public string? ApplianceSummary { get; set; }
    public decimal? PredictedNextAmt { get; set; }
    public string? PredictionBasis { get; set; }
    public string? SavingsTip { get; set; }
    public DateTime ComputedAt { get; set; }
}
