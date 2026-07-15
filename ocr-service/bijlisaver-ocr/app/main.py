"""BijliSaver OCR microservice.

One job: bill image in, validated JSON out.
All business logic (slab analysis, predictions, explanations) lives in
the ASP.NET Core backend — not here.

Run:  uvicorn app.main:app --reload --port 8001
"""

import asyncio
import logging
from dotenv import load_dotenv
load_dotenv()
from fastapi import FastAPI, File, HTTPException, UploadFile

from . import validation, vision
from .schemas import ExtractionResponse

logging.basicConfig(level=logging.INFO)
log = logging.getLogger("bijlisaver.ocr")

ALLOWED_TYPES = {"image/jpeg", "image/png", "image/webp", "application/pdf"}
MAX_UPLOAD_BYTES = 10 * 1024 * 1024  # 10 MB

app = FastAPI(title="BijliSaver OCR Service", version="0.1.0")


@app.get("/health")
def health() -> dict:
    return {"status": "ok"}


@app.post("/extract", response_model=ExtractionResponse)
async def extract(file: UploadFile = File(...)) -> ExtractionResponse:
    if file.content_type not in ALLOWED_TYPES:
        raise HTTPException(415, f"Unsupported file type: {file.content_type}")

    image_bytes = await file.read()
    if len(image_bytes) > MAX_UPLOAD_BYTES:
        raise HTTPException(413, "File too large (max 10 MB)")
    if not image_bytes:
        raise HTTPException(400, "Empty file")

    try:
        data = await asyncio.to_thread(vision.extract_bill, image_bytes, file.content_type)
    except vision.NotABillError:
        return ExtractionResponse(status="not_a_bill")
    except vision.ExtractionFailedError as e:
        log.error("Extraction failed: %s", e)
        return ExtractionResponse(status="failed")

    effective_confidence, issues = validation.validate(data)
    status = validation.status_from_confidence(effective_confidence)

    for issue in issues:
        log.warning("Validation [%s] %s: %s", status, issue.check, issue.detail)

    return ExtractionResponse(
        status=status,
        data=data,
        effective_confidence=effective_confidence,
        issues=issues,
    )
from .advice import AdviceRequest, AdviceResponse, AdviceFailedError, generate_advice


@app.post("/advise", response_model=AdviceResponse)
async def advise(facts: AdviceRequest) -> AdviceResponse:
    try:
        return await asyncio.to_thread(generate_advice, facts)
    except AdviceFailedError as e:
        log.error("Advice generation failed: %s", e)
        raise HTTPException(502, "Could not generate advice")