// Maps raw OCR labels ("F.C SURCHARGE", "QTR TARRIF ADJ/DMC") to
// charge_definitions codes. Built from labels found on 3 real LESCO bills.
//
// Normalization: uppercase, keep only letters/digits — so "F.C Surcharge",
// "FC SURCHARGE" and "F.C. SURCHARGE" all become "FCSURCHARGE".
// Unmatched labels return null → stored with charge_def_id = NULL →
// shown as "Other charge" in the UI and logged so we can grow this table.

namespace BijliSaver.Api.Services;

public static class ChargeLabelMapper
{
    private static readonly Dictionary<string, string?> Map = new()
    {
        // Metadata rows — not charges, skip entirely (mapper returns "SKIP")
        ["UNITSCONSUMED"] = "SKIP",
        ["TOTAL"] = "SKIP",
        ["TOTALTAXESONFPA"] = "SKIP",          // subtotal — would double-count

        // LESCO charges column
        ["COSTOFELECTRICITY"] = "ENERGY",
        ["METERRENT"] = "METER_RENT",
        ["SERVICERENT"] = "SERVICE_RENT",
        ["FUELPRICEADJUSTMENT"] = "FPA",
        ["FPA"] = "FPA",
        ["FCSURCHARGE"] = "FC_SURCHARGE",
        ["QUARTERLYTARIFFADJUSTMENT"] = "QTR_ADJ",
        ["QTRTARRIFADJDMC"] = "QTR_ADJ",       // LESCO's own misspelling, seen on a real bill
        ["QTRTRFADJ"] = "QTR_ADJ",

        // Govt charges column
        ["ELECTRICITYDUTY"] = "ED",
        ["TVFEE"] = "TVFEE",
        ["GST"] = "GST",
        ["INCOMETAX"] = "INCOME_TAX",
        ["EXTRATAX"] = "EXTRA_TAX",
        ["FURTHERTAX"] = "FURTHER_TAX",
        ["RSTAX"] = "RS_TAX",
        ["RETAILERSTAX"] = "RS_TAX",
        ["GSTONFPA"] = "GST_ON_FPA",
        ["EDONFPA"] = "ED_ON_FPA",
        ["STAXONFPA"] = "OTHER_FPA_TAX",
        ["ITONFPA"] = "OTHER_FPA_TAX",
        ["ETONFPA"] = "OTHER_FPA_TAX",
        ["RSTAXONFPA"] = "OTHER_FPA_TAX",
        ["FURTHERTAXONFPA"] = "OTHER_FPA_TAX",
        ["INCOMETAXONFPA"] = "OTHER_FPA_TAX",

        // Totals column / misc
        ["LPSURCHARGE"] = "LP_SURCHARGE",
        ["BILLADJUSTMENT"] = "ARREARS",
        ["ARREARAGE"] = "ARREARS",
        ["DEFERREDAMOUNT"] = "DEFERRED",
    };

    public static string Normalize(string rawLabel) =>
        new(rawLabel.ToUpperInvariant().Where(char.IsLetterOrDigit).ToArray());

    /// <returns>
    /// The charge code, or "SKIP" for non-charge rows, or null for
    /// unrecognized labels (store as "Other charge" and log).
    /// </returns>
    public static string? MapToCode(string rawLabel) =>
        Map.TryGetValue(Normalize(rawLabel), out var code) ? code : null;
}
