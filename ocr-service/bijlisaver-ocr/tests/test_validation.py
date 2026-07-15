"""Validation tests built from the 3 real LESCO bills reviewed in Sprint 0.

Bill 1: Mar-24 — heavy arrears (Rs 64,359), normal positive FPA
Bill 2: Dec-25 — NEGATIVE FPA (-130.48), estimated 'EX' history rows
Bill 3: Jun-24 — pro-rata reading note, mid-size bill

Run:  python -m pytest tests/ -v      (or: python tests/test_validation.py)
"""

import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

from app.schemas import BillExtraction, ChargeLine, HistoryEntry
from app.validation import validate, status_from_confidence


def bill_1_mar24() -> BillExtraction:
    """The arrears bill: current 5,196.41 but payable 70,452."""
    return BillExtraction(
        reference_no="12 11221 0800831U",
        customer_id="9962687",
        billing_month="2024-03",
        units_consumed=141,
        previous_reading=3612,
        current_reading=3753,
        charges=[
            ChargeLine(raw_label="COST OF ELECTRICITY", amount=3236.21),
            ChargeLine(raw_label="FUEL PRICE ADJUSTMENT", amount=747.96),
            ChargeLine(raw_label="F.C SURCHARGE", amount=455.43),
            ChargeLine(raw_label="QUARTERLY TARIFF ADJUSTMENT", amount=624.86),
            ChargeLine(raw_label="ELECTRICITY DUTY", amount=57.91),
            ChargeLine(raw_label="TV FEE", amount=35),
            ChargeLine(raw_label="GST", amount=787),
            ChargeLine(raw_label="GST ON FPA", amount=137),
            ChargeLine(raw_label="ED ON FPA", amount=11.22),
        ],
        lesco_charges_total=5064.46,
        govt_charges_total=1028.13,
        current_bill=5196.41,
        arrears=64359.00,
        total_fpa=896.18,
        fpa_rate=7.0562,
        fpa_month="2024-01",
        gop_rate=22.950,
        payable_within_due=70452,
        payable_after_due=70884,
        lp_surcharge=432,
        due_date="2024-04-08",
        history=[
            HistoryEntry(month="2024-01", units=106),   # FPA month — verifies identity D
            HistoryEntry(month="2024-02", units=192),
        ],
        confidence=0.95,
    )


def bill_2_dec25() -> BillExtraction:
    """The negative-FPA bill with estimated history rows."""
    return BillExtraction(
        customer_id="8237138",
        tariff_code="A-1a(01)",
        billing_month="2025-12",
        units_consumed=139,
        previous_reading=16624,
        current_reading=16763,
        charges=[
            ChargeLine(raw_label="COST OF ELECTRICITY", amount=4019),
            ChargeLine(raw_label="FUEL PRICE ADJUSTMENT", amount=-130.48),
            ChargeLine(raw_label="F.C SURCHARGE", amount=448.97),
            ChargeLine(raw_label="QTR TARRIF ADJ/DMC", amount=29.03),
            ChargeLine(raw_label="ELECTRICITY DUTY", amount=61),
            ChargeLine(raw_label="GST", amount=820),
            ChargeLine(raw_label="TOTAL TAXES ON FPA", amount=-25.96),
        ],
        lesco_charges_total=4366.52,
        govt_charges_total=855.04,
        current_bill=5377.44,
        arrears=0,
        total_fpa=-156,
        fpa_rate=-0.8816,
        fpa_month="2025-10",
        gop_rate=28.910,
        payable_within_due=5221,
        payable_after_due=5671,
        due_date="2026-01-06",
        history=[
            HistoryEntry(month="2025-02", units=68, estimated=True),   # "EX 68"
            HistoryEntry(month="2025-05", units=177, estimated=True),  # "EX 177"
            HistoryEntry(month="2025-10", units=148, estimated=True),  # "EX 148" — FPA month
            HistoryEntry(month="2025-11", units=146, estimated=False),
        ],
        confidence=0.95,
    )


def bill_3_jun24() -> BillExtraction:
    """The pro-rata bill — reading check must be skipped."""
    return BillExtraction(
        billing_month="2024-06",
        units_consumed=206,
        previous_reading=2404,
        current_reading=2610,
        reading_is_estimated=True,  # "Pro-Rata based Present Reading" note
        charges=[
            ChargeLine(raw_label="COST OF ELECTRICITY", amount=5590.38),
            ChargeLine(raw_label="FUEL PRICE ADJUSTMENT", amount=289.89),
            ChargeLine(raw_label="F.C SURCHARGE", amount=665.38),
            ChargeLine(raw_label="QUARTERLY TARIFF ADJUSTMENT", amount=818.86),
            ChargeLine(raw_label="ELECTRICITY DUTY", amount=96.15),
            ChargeLine(raw_label="TV FEE", amount=35),
            ChargeLine(raw_label="GST", amount=1291),
            ChargeLine(raw_label="GST ON FPA", amount=53),
            ChargeLine(raw_label="ED ON FPA", amount=4.35),
        ],
        lesco_charges_total=7364.51,
        govt_charges_total=1479.50,
        current_bill=8496.77,
        arrears=0,
        total_fpa=347.24,
        fpa_rate=3.3221,
        fpa_month="2024-04",
        gop_rate=27.140,
        payable_within_due=8844,
        payable_after_due=9551,
        lp_surcharge=707,
        history=[
            HistoryEntry(month="2024-04", units=87),    # FPA month — verifies identity D
            HistoryEntry(month="2024-05", units=176),
        ],
        confidence=0.95,
    )


# ---------------------------------------------------------------- tests

def test_bill_1_passes_all_identities():
    conf, issues = validate(bill_1_mar24())
    errors = [i for i in issues if i.severity == "error"]
    assert errors == [], f"Unexpected errors: {errors}"
    assert conf == 0.95
    assert status_from_confidence(conf) == "done"


def test_bill_2_negative_fpa_passes():
    conf, issues = validate(bill_2_dec25())
    errors = [i for i in issues if i.severity == "error"]
    assert errors == [], f"Unexpected errors: {errors}"
    assert status_from_confidence(conf) == "done"


def test_bill_3_prorata_skips_reading_check():
    conf, issues = validate(bill_3_jun24())
    errors = [i for i in issues if i.severity == "error"]
    assert errors == [], f"Unexpected errors: {errors}"
    assert status_from_confidence(conf) == "done"


def test_hallucinated_total_is_caught():
    """Corrupt one digit the way OCR errors actually happen: 5196.41 -> 5916.41."""
    bill = bill_1_mar24()
    bill.current_bill = 5916.41
    conf, issues = validate(bill)
    failed_checks = {i.check for i in issues if i.severity == "error"}
    # Both column identity and payable identity should break
    assert "identity_A_columns" in failed_checks
    assert "identity_B_payable" in failed_checks
    assert conf == 0.6
    assert status_from_confidence(conf) == "needs_review"


def test_hallucinated_units_is_caught():
    """Misread units (141 -> 741) breaks rate math AND meter delta."""
    bill = bill_1_mar24()
    bill.units_consumed = 741
    conf, issues = validate(bill)
    failed_checks = {i.check for i in issues if i.severity == "error"}
    assert "identity_C_energy" in failed_checks
    assert "reading_delta" in failed_checks
    assert status_from_confidence(conf) == "needs_review"


def test_wrong_reading_caught_on_normal_bill():
    bill = bill_1_mar24()
    bill.current_reading = 3853  # misread digit: delta 241 != 141 units
    conf, issues = validate(bill)
    assert any(i.check == "reading_delta" for i in issues)
    assert status_from_confidence(conf) == "needs_review"


def test_missing_fields_do_not_crash():
    """A partial extraction (nulls everywhere) should validate without errors."""
    bill = BillExtraction(confidence=0.5)
    conf, issues = validate(bill)
    assert conf == 0.5
    assert status_from_confidence(conf) == "needs_review"  # low confidence alone


if __name__ == "__main__":
    # Zero-dependency runner (no pytest needed)
    failures = 0
    for name, fn in sorted(globals().items()):
        if name.startswith("test_") and callable(fn):
            try:
                fn()
                print(f"  PASS  {name}")
            except AssertionError as e:
                failures += 1
                print(f"  FAIL  {name}: {e}")
    print(f"\n{'ALL TESTS PASSED' if failures == 0 else f'{failures} FAILURES'}")
    sys.exit(1 if failures else 0)
