# BijliSaver

Electricity bills in Pakistan are confusing — a dozen line items, taxes
mixed in with usage, slab pricing that quietly reprices your whole bill
once you cross a threshold. BijliSaver reads a photo of your bill and
explains every charge in plain language, warns you before you cross into a
more expensive pricing slab, and gives you a savings plan based on your
actual usage.

Currently tuned and verified for **LESCO** (Lahore) bills; **MEPCO**
(Multan) has also read correctly in testing, which tracks since Punjab
DISCOs share the same billing system. Other DISCOs aren't verified yet.

## How it works

1. **Upload** a photo or PDF of your bill.
2. A **vision model** extracts every line item as structured data.
3. Four **arithmetic identities** that every LESCO bill satisfies (e.g.
   LESCO charges + Govt charges = Current bill + Total FPA) are checked
   against the extraction — any failure caps confidence and asks for a
   clearer photo instead of showing wrong numbers.
4. The backend maps each charge to a plain-language explanation, computes
   your position in the tariff slab, and predicts next month's bill from
   your 12-month usage history.
5. An AI advice pass turns those facts into a personalized savings plan —
   specific appliance habits and the rupee amount they'd save you, not
   generic tips.

## Architecture

```
bijlisaver-web (React + Vite)
        |
        v  /api/*
BijliSaver.Api (ASP.NET Core)  <---calls--->  bijlisaver-ocr (FastAPI)
        |
        v
   PostgreSQL
```

- **bijlisaver-web** — the frontend. Upload screen, per-bill breakdown, bill
  history with a usage chart.
- **BijliSaver.Api** — orchestrates the flow: calls the OCR service, maps
  raw charge labels to explanations, computes slab/tax/prediction insights,
  persists everything.
- **ocr-service/bijlisaver-ocr** — a single-purpose FastAPI service: bill
  image in, validated structured JSON out. Never trusts the vision model's
  output blindly (see its own README for the identity checks).

Each has its own README with local setup instructions:
[bijlisaver-web](bijlisaver-web/README.md) ·
[BijliSaver.Api](BijliSaver.Api/README.md) ·
[bijlisaver-ocr](ocr-service/bijlisaver-ocr/README.md)

## Tech stack

| Layer | Stack |
|---|---|
| Frontend | React, Vite, Tailwind CSS |
| Backend | ASP.NET Core, Entity Framework Core |
| OCR / AI | FastAPI, Gemini (via an OpenAI-compatible endpoint) |
| Database | PostgreSQL |

## Running locally

Each service needs to be started separately — see the linked READMEs above
for exact steps. Order: Postgres → OCR service → API → web.

## Deployment

See [DEPLOY.md](DEPLOY.md) for a full free-tier deployment guide (Render +
Neon).
