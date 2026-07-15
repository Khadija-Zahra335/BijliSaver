"""OpenAI Vision call: image in, parsed BillExtraction out.

This module does NOT validate business logic — that's validation.py's job.
It only handles: API call, fence-stripping, JSON parsing, not_a_bill detection.
"""

import base64
import json
import os

from openai import OpenAI
from pydantic import ValidationError

from .prompt import SYSTEM_PROMPT, USER_PROMPT
from .schemas import BillExtraction

MODEL = os.getenv("OCR_MODEL", "gpt-4o-mini")

_client: OpenAI | None = None


def _get_client() -> OpenAI:
    global _client
    if _client is None:
              _client = OpenAI(base_url=os.getenv("OPENAI_BASE_URL"))
    return _client


class NotABillError(Exception):
    pass


class ExtractionFailedError(Exception):
    pass


def _strip_fences(text: str) -> str:
    """Models occasionally wrap JSON in ```json fences despite instructions."""
    text = text.strip()
    if text.startswith("```"):
        text = text.split("\n", 1)[1] if "\n" in text else text
        if text.endswith("```"):
            text = text[: -3]
    return text.strip()


def extract_bill(image_bytes: bytes, content_type: str = "image/jpeg") -> BillExtraction:
    """Send bill image to the Vision model, return parsed extraction.

    Raises NotABillError if the image isn't a bill,
    ExtractionFailedError on unparseable output.
    """
    b64 = base64.b64encode(image_bytes).decode()

    response = _get_client().chat.completions.create(
        model=MODEL,
        temperature=0,  # extraction, not creativity
        messages=[
            {"role": "system", "content": SYSTEM_PROMPT},
            {
                "role": "user",
                "content": [
                    {"type": "image_url",
                     "image_url": {"url": f"data:{content_type};base64,{b64}"}},
                    {"type": "text", "text": USER_PROMPT},
                ],
            },
        ],
    )

    raw = _strip_fences(response.choices[0].message.content or "")

    try:
        payload = json.loads(raw)
    except json.JSONDecodeError as e:
        raise ExtractionFailedError(f"Model returned invalid JSON: {e}") from e

    if payload.get("error") == "not_a_bill":
        raise NotABillError("Image is not an electricity bill")
	
    payload["charges"] = [
        c for c in (payload.get("charges") or [])
        if isinstance(c, dict) and c.get("amount") is not None
    ]
    try:
        return BillExtraction(**payload)
    except ValidationError as e:
        raise ExtractionFailedError(f"JSON does not match schema: {e}") from e
