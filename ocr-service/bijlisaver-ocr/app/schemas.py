"""Pydantic models for the OCR extraction result.

Mirrors the JSON structure defined in ocr_prompt_design_v2.md.
Negative amounts are valid on FPA-related fields (real bills show e.g. -130.48).
"""

from datetime import date
from pydantic import BaseModel, Field


class ChargeLine(BaseModel):
    raw_label: str = Field(..., max_length=120)
    amount: float  # negatives allowed (fuel adjustment refunds)


class HistoryEntry(BaseModel):
    month: str                      # "YYYY-MM"
    units: int | None = None
    amount: float | None = None
    estimated: bool = False         # "EX 177" rows on real bills


class BillExtraction(BaseModel):
    """What the Vision model must return."""
    disco: str = "LESCO"
    reference_no: str | None = None
    customer_id: str | None = None
    tariff_code: str | None = None          # e.g. "A-1a(01)"
    billing_month: str | None = None        # "YYYY-MM"
    units_consumed: int | None = None
    previous_reading: int | None = None
    current_reading: int | None = None
    reading_is_estimated: bool = False

    charges: list[ChargeLine] = []
    lesco_charges_total: float | None = None
    govt_charges_total: float | None = None

    current_bill: float | None = None
    arrears: float | None = None
    total_fpa: float | None = None          # may be negative
    fpa_rate: float | None = None
    fpa_month: str | None = None            # "YYYY-MM" — FPA applies to a PAST month's units (printed "FPA JAN-24 @ 7.0562")
    gop_rate: float | None = None
    payable_within_due: float | None = None
    payable_after_due: float | None = None
    lp_surcharge: float | None = None
    due_date: str | None = None             # "YYYY-MM-DD"

    history: list[HistoryEntry] = []
    confidence: float = Field(0.0, ge=0.0, le=1.0)


class ValidationIssue(BaseModel):
    check: str          # which identity/check failed
    detail: str         # human-readable explanation
    severity: str       # "error" (caps confidence) | "warning" (logged only)


class ExtractionResponse(BaseModel):
    """What the FastAPI service returns to the .NET backend."""
    status: str                     # "done" | "needs_review" | "failed" | "not_a_bill"
    data: BillExtraction | None = None
    effective_confidence: float = 0.0
    issues: list[ValidationIssue] = []
