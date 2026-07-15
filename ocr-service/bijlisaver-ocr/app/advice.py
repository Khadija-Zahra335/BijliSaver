"""Personalized savings advice generation.

Second use of the LLM: after the .NET backend computes the FACTS
(slab position, tax split, history trend), it sends them here and we
ask the model to write a personalized plan in simple words.

Safety rule: the model may only use numbers provided in the facts.
It explains and advises — it never invents rupee amounts.
"""

import json
import os

from openai import OpenAI
from pydantic import BaseModel

MODEL = os.getenv("OCR_MODEL", "gemini-2.5-flash")

_client: OpenAI | None = None


def _get_client() -> OpenAI:
    global _client
    if _client is None:
        _client = OpenAI(base_url=os.getenv("OPENAI_BASE_URL"))
    return _client


class AdviceRequest(BaseModel):
    """Facts computed by the .NET backend. All optional — send what exists."""
    billing_month: str | None = None          # "2024-06"
    units_consumed: int | None = None
    current_bill: float | None = None
    tax_total: float | None = None
    tax_percentage: float | None = None
    slab_ceiling: int | None = None           # upper bound of current slab (e.g. 300)
    units_to_next_slab: int | None = None
    next_slab_penalty: float | None = None    # Rs jump if crossed
    units_to_lower_slab: int | None = None    # units to cut to DROP a slab
    lower_slab_saving: float | None = None    # Rs saved by dropping
    rate_per_unit: float | None = None
    avg_recent_units: float | None = None     # avg of last 3 non-estimated months
    trend: str | None = None                  # "rising" | "falling" | "stable"
    season_hint: str | None = None            # "summer" | "winter" | None
    has_arrears: bool = False
    fpa_is_negative: bool = False


class AdviceResponse(BaseModel):
    summary: str                               # 1-2 sentence situation summary
    tips: list[str]                            # 3-5 concrete, personalized tips


SYSTEM_PROMPT = """You are a friendly electricity-saving advisor for households in Pakistan.
You receive FACTS about one family's electricity bill, computed by software.

Write a short personalized savings plan. Rules:
1. Respond with ONLY valid JSON: {"summary": "...", "tips": ["...", "..."]}
2. Simple words a non-technical person understands. No jargon, no abbreviations.
   If you must use a term like "slab", explain it in the same sentence.
3. Use ONLY the rupee amounts and unit numbers given in the facts.
   NEVER invent or estimate any number that is not in the facts.
4. Make tips SPECIFIC to this bill's situation, not generic advice:
   - If close to the next slab: lead with that warning and what staying under saves.
   - If cutting some units would DROP a slab: say exactly how many units and the saving.
   - Use the trend: rising usage deserves different advice than falling.
   - In summer, focus on AC/fan habits (AC at 26°C, fans instead of AC at night,
     servicing AC filters). In winter, focus on water heating and heaters.
   - If usage is already low, say so honestly — do not push pointless cuts.
5. 3 to 5 tips. Each tip is 1-2 sentences. Practical actions for a Pakistani home
   (AC temperature, iron in batches, inverter appliances, off-peak habits,
   phantom loads, servicing).
6. Never mention arrears repayment plans or anything about unpaid bills — if
   has_arrears is true, you may gently note that paying on time avoids penalty,
   nothing more.
7. Warm, respectful tone. No lecturing."""


class AdviceFailedError(Exception):
    pass


def generate_advice(facts: AdviceRequest) -> AdviceResponse:
    response = _get_client().chat.completions.create(
        model=MODEL,
        temperature=0.4,   # a little variety in phrasing, facts stay fixed
        messages=[
            {"role": "system", "content": SYSTEM_PROMPT},
            {"role": "user", "content": "FACTS:\n" + facts.model_dump_json(exclude_none=True)},
        ],
    )

    raw = (response.choices[0].message.content or "").strip()
    if raw.startswith("```"):
        raw = raw.split("\n", 1)[1] if "\n" in raw else raw
        if raw.endswith("```"):
            raw = raw[:-3]
        raw = raw.strip()

    try:
        payload = json.loads(raw)
        return AdviceResponse(**payload)
    except Exception as e:
        raise AdviceFailedError(f"Model returned invalid advice JSON: {e}") from e
