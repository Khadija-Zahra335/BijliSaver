"""Post-extraction validation.

Never trust model output blindly. LESCO bills follow strict arithmetic
identities (verified against 3 real bills):

  A. lesco_charges_total + govt_charges_total == current_bill + total_fpa
  B. current_bill + arrears + total_fpa == payable_within_due
  C. gop_rate * units_consumed == cost-of-electricity charge line
  D. fpa_rate * units_consumed == fuel-price-adjustment charge line

A hallucinated number almost always breaks one of these. A failed identity
caps confidence at 0.6 -> status "needs_review" -> user is asked to retake
the photo instead of being shown wrong numbers.
"""

from .schemas import BillExtraction, ValidationIssue

# Tolerances in rupees. Bills round to whole rupees on payable lines.
TOL_COLUMN = 5.0      # identities A & B
TOL_RATE = 2.0        # identities C & D
CONFIDENCE_CAP_ON_ERROR = 0.6
REVIEW_THRESHOLD = 0.7

# Normalized label fragments used to find specific charge lines.
_ENERGY_LABELS = {"COSTOFELECTRICITY"}
_FPA_LABELS = {"FUELPRICEADJUSTMENT", "FPA"}


def normalize_label(label: str) -> str:
    """Uppercase and strip separators so 'F.C Surcharge' == 'FC SURCHARGE'."""
    return "".join(ch for ch in label.upper() if ch.isalnum())


def _find_charge(data: BillExtraction, labels: set[str]) -> float | None:
    for line in data.charges:
        if normalize_label(line.raw_label) in labels:
            return line.amount
    return None


def _check(name: str, lhs: float, rhs: float, tol: float, detail: str) -> ValidationIssue | None:
    if abs(lhs - rhs) > tol:
        return ValidationIssue(
            check=name,
            detail=f"{detail}: {lhs:.2f} vs {rhs:.2f} (diff {abs(lhs - rhs):.2f}, tolerance {tol})",
            severity="error",
        )
    return None


def validate(data: BillExtraction) -> tuple[float, list[ValidationIssue]]:
    """Run all checks. Returns (effective_confidence, issues)."""
    issues: list[ValidationIssue] = []

    # --- Identity A: column math -----------------------------------------
    if None not in (data.lesco_charges_total, data.govt_charges_total,
                    data.current_bill, data.total_fpa):
        issue = _check(
            "identity_A_columns",
            data.lesco_charges_total + data.govt_charges_total,
            data.current_bill + data.total_fpa,
            TOL_COLUMN,
            "LESCO total + Govt total should equal Current bill + Total FPA",
        )
        if issue:
            issues.append(issue)

    # --- Identity B: payable math -----------------------------------------
    if None not in (data.current_bill, data.payable_within_due):
        arrears = data.arrears or 0.0
        fpa = data.total_fpa or 0.0
        issue = _check(
            "identity_B_payable",
            data.current_bill + arrears + fpa,
            data.payable_within_due,
            TOL_COLUMN,
            "Current bill + Arrears + Total FPA should equal Payable within due date",
        )
        if issue:
            issues.append(issue)

    # --- Identity C: energy math -------------------------------------------
    energy = _find_charge(data, _ENERGY_LABELS)
    if None not in (data.gop_rate, data.units_consumed) and energy is not None:
        issue = _check(
            "identity_C_energy",
            data.gop_rate * data.units_consumed,
            energy,
            TOL_RATE,
            "Tariff rate x units should equal Cost of Electricity",
        )
        if issue:
            issues.append(issue)

    # --- Identity D: FPA math -------------------------------------------
    # DISCOVERED FROM REAL BILLS: FPA is applied retroactively to a PAST
    # month's units (printed as "FPA JAN-24 @ 7.0562"), NOT the current
    # month's. Verified: Bill Mar-24 FPA 747.96 / 7.0562 = 106 units
    # = exactly Jan-24's units in the bill's own history table.
    # So we check rate x history[fpa_month].units == FPA line.
    fpa_line = _find_charge(data, _FPA_LABELS)
    if data.fpa_rate is not None and data.fpa_month is not None and fpa_line is not None:
        hist_units = next(
            (h.units for h in data.history if h.month == data.fpa_month and h.units is not None),
            None,
        )
        if hist_units is not None:
            issue = _check(
                "identity_D_fpa",
                data.fpa_rate * hist_units,
                fpa_line,
                TOL_RATE,
                f"FPA rate x units of {data.fpa_month} should equal Fuel Price Adjustment line",
            )
            if issue:
                issues.append(issue)
        else:
            # FPA month not readable in history — can't verify, note it but don't punish.
            issues.append(ValidationIssue(
                check="identity_D_fpa",
                detail=f"FPA month {data.fpa_month} not found in history table; check skipped",
                severity="warning",
            ))

    # --- Reading check (skip on estimated/pro-rata bills) --------------------
    if (not data.reading_is_estimated
            and None not in (data.previous_reading, data.current_reading, data.units_consumed)):
        delta = data.current_reading - data.previous_reading
        if abs(delta - data.units_consumed) > 1:
            issues.append(ValidationIssue(
                check="reading_delta",
                detail=(f"Meter delta {delta} does not match units consumed "
                        f"{data.units_consumed}"),
                severity="error",
            ))

    # --- Sanity ranges ---------------------------------------------------------
    if data.units_consumed is not None and not (0 <= data.units_consumed <= 10_000):
        issues.append(ValidationIssue(
            check="range_units", detail=f"Units out of range: {data.units_consumed}",
            severity="error"))
    if data.current_bill is not None and not (0 <= data.current_bill <= 2_000_000):
        issues.append(ValidationIssue(
            check="range_bill", detail=f"Current bill out of range: {data.current_bill}",
            severity="error"))

    # --- Effective confidence -----------------------------------------------
    effective = data.confidence
    if any(i.severity == "error" for i in issues):
        effective = min(effective, CONFIDENCE_CAP_ON_ERROR)
    return effective, issues


def status_from_confidence(effective_confidence: float) -> str:
    return "done" if effective_confidence >= REVIEW_THRESHOLD else "needs_review"
