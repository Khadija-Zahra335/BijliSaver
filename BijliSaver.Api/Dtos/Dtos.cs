// DTOs mirroring the FastAPI OCR service's ExtractionResponse.
// Property names match the Python JSON via JsonPropertyName.

using System.Text.Json.Serialization;

namespace BijliSaver.Api.Dtos;

public class OcrExtractionResponse
{
    [JsonPropertyName("status")] public string Status { get; set; } = "";
    [JsonPropertyName("data")] public OcrBillData? Data { get; set; }
    [JsonPropertyName("effective_confidence")] public decimal EffectiveConfidence { get; set; }
    [JsonPropertyName("issues")] public List<OcrIssue> Issues { get; set; } = [];
}

public class OcrIssue
{
    [JsonPropertyName("check")] public string Check { get; set; } = "";
    [JsonPropertyName("detail")] public string Detail { get; set; } = "";
    [JsonPropertyName("severity")] public string Severity { get; set; } = "";
}

public class OcrBillData
{
    [JsonPropertyName("disco")] public string Disco { get; set; } = "LESCO";
    [JsonPropertyName("reference_no")] public string? ReferenceNo { get; set; }
    [JsonPropertyName("customer_id")] public string? CustomerId { get; set; }
    [JsonPropertyName("tariff_code")] public string? TariffCode { get; set; }
    [JsonPropertyName("billing_month")] public string? BillingMonth { get; set; }   // "YYYY-MM"
    [JsonPropertyName("units_consumed")] public int? UnitsConsumed { get; set; }
    [JsonPropertyName("previous_reading")] public int? PreviousReading { get; set; }
    [JsonPropertyName("current_reading")] public int? CurrentReading { get; set; }
    [JsonPropertyName("reading_is_estimated")] public bool ReadingIsEstimated { get; set; }
    [JsonPropertyName("charges")] public List<OcrCharge> Charges { get; set; } = [];
    [JsonPropertyName("lesco_charges_total")] public decimal? LescoChargesTotal { get; set; }
    [JsonPropertyName("govt_charges_total")] public decimal? GovtChargesTotal { get; set; }
    [JsonPropertyName("current_bill")] public decimal? CurrentBill { get; set; }
    [JsonPropertyName("arrears")] public decimal? Arrears { get; set; }
    [JsonPropertyName("total_fpa")] public decimal? TotalFpa { get; set; }
    [JsonPropertyName("fpa_rate")] public decimal? FpaRate { get; set; }
    [JsonPropertyName("fpa_month")] public string? FpaMonth { get; set; }           // "YYYY-MM"
    [JsonPropertyName("gop_rate")] public decimal? GopRate { get; set; }
    [JsonPropertyName("payable_within_due")] public decimal? PayableWithinDue { get; set; }
    [JsonPropertyName("payable_after_due")] public decimal? PayableAfterDue { get; set; }
    [JsonPropertyName("lp_surcharge")] public decimal? LpSurcharge { get; set; }
    [JsonPropertyName("due_date")] public string? DueDate { get; set; }             // "YYYY-MM-DD"
    [JsonPropertyName("history")] public List<OcrHistoryEntry> History { get; set; } = [];
    [JsonPropertyName("confidence")] public decimal Confidence { get; set; }
}

public class OcrCharge
{
    [JsonPropertyName("raw_label")] public string RawLabel { get; set; } = "";
    [JsonPropertyName("amount")] public decimal Amount { get; set; }
}

public class OcrHistoryEntry
{
    [JsonPropertyName("month")] public string Month { get; set; } = "";             // "YYYY-MM"
    [JsonPropertyName("units")] public int? Units { get; set; }
    [JsonPropertyName("amount")] public decimal? Amount { get; set; }
    [JsonPropertyName("estimated")] public bool Estimated { get; set; }
}

// ---------- What our API returns to the React frontend ----------

public record BillSummaryDto(
    Guid Id,
    string BillingMonth,
    int? UnitsConsumed,
    decimal? CurrentBill,
    decimal? PayableWithinDue,
    string OcrStatus);

public record ChargeLineDto(
    string DisplayName,      // "Fuel Price Adjustment" (or raw label if unrecognized)
    decimal Amount,
    string? Explanation,     // the plain-language line — never null for recognized charges
    string Category);        // energy|tax|surcharge|fee|adjustment|penalty|other

public record BillDetailDto(
    Guid Id,
    string BillingMonth,
    int? UnitsConsumed,
    decimal? CurrentBill,
    decimal? Arrears,
    decimal? TotalFpa,
    decimal? PayableWithinDue,
    decimal? PayableAfterDue,
    string? DueDate,
    string OcrStatus,
    List<ChargeLineDto> Charges,
    InsightDto? Insight);

public record InsightDto(
    int? SlabReached,
    int? UnitsToNextSlab,
    decimal? NextSlabPenalty,
    decimal? TaxTotal,
    decimal? TaxPercentage,
    bool HasArrears,
    string? ArrearsNote,     // the honest "this is old bills, not taxes" message
    string? ApplianceSummary,
    decimal? PredictedNextAmt,
    string? SavingsTip);

public class AdviceResponse
{
    [JsonPropertyName("summary")] public string Summary { get; set; } = "";
    [JsonPropertyName("tips")] public List<string> Tips { get; set; } = [];
}
