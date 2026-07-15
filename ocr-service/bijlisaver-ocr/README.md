# BijliSaver OCR Service

FastAPI microservice with one job: LESCO bill image in, validated JSON out.
All business logic (slab analysis, predictions) lives in the ASP.NET Core backend.

## Setup

```bash
python -m venv venv
venv\Scripts\activate          # Windows  (source venv/bin/activate on Linux/Mac)
pip install -r requirements.txt
copy .env.example .env         # then put your OpenAI key in .env
```

## Run

```bash
uvicorn app.main:app --reload --port 8001
```

Open http://localhost:8001/docs — upload a bill photo to POST /extract.

## Test (no API key needed)

```bash
python -m pytest tests/ -v
```

The test suite encodes 3 real LESCO bills (Mar-24, Jun-24, Dec-25 formats)
plus hallucination scenarios. It runs entirely offline.

## How trust works here

The Vision model's output is never trusted blindly. Four arithmetic
identities that every LESCO bill satisfies are checked after extraction:

| Identity | Check |
|----------|-------|
| A | LESCO total + Govt total = Current bill + Total FPA |
| B | Current bill + Arrears + Total FPA = Payable within due date |
| C | Tariff rate x units = Cost of Electricity |
| D | FPA rate x (units of the FPA's reference month, from the bill's own history table) = FPA line |

Any failed identity caps confidence at 0.6 → status `needs_review` →
the user is asked to retake the photo instead of being shown wrong numbers.

Identity D is subtle: fuel price adjustment is applied retroactively to a
PAST month's consumption ("FPA JAN-24 @ 7.0562" on a March bill). This was
discovered when the original check failed against all 3 real bills.
