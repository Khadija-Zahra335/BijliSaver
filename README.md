# BijliSaver

**Understand your electricity bill in plain language — and know exactly how to lower it.**

[![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)](BijliSaver.Api)
[![React](https://img.shields.io/badge/React-18-61DAFB?logo=react&logoColor=white)](bijlisaver-web)
[![FastAPI](https://img.shields.io/badge/FastAPI-Python%203.12-009688?logo=fastapi&logoColor=white)](ocr-service/bijlisaver-ocr)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-Neon-4169E1?logo=postgresql&logoColor=white)](#tech-stack)
[![Deployed on Render](https://img.shields.io/badge/Deployed-Render-46E3B7?logo=render&logoColor=white)](#deployment)

Electricity bills in Pakistan pack a dozen line items into one number — usage,
fuel adjustments, government taxes, surcharges — with slab pricing that
quietly reprices the *entire* bill once you cross a threshold. BijliSaver
turns a photo of that bill into a breakdown anyone can read, flags exactly
how close you are to the next pricing slab, and generates a savings plan
tied to your actual appliance usage and rupee amounts — not generic advice.

Currently tuned and verified for **LESCO** (Lahore) bills. **MEPCO**
(Multan) has also read correctly in testing, consistent with Punjab DISCOs
sharing the same billing system; other DISCOs are not yet verified.

## Contents

- [How it works](#how-it-works)
- [Architecture](#architecture)
- [Tech stack](#tech-stack)
- [Project structure](#project-structure)
- [Local development](#local-development)
- [Deployment](#deployment)
- [API overview](#api-overview)
- [Known limitations](#known-limitations)

## How it works

1. **Upload** a photo or PDF of your bill — no account required.
2. A **vision model** extracts every line item into structured data.
3. Four **arithmetic identities** that every LESCO bill satisfies are
   checked against the extraction before anything is trusted:

   | Identity | Check |
   |---|---|
   | A | LESCO charges total + Govt charges total = Current bill + Total FPA |
   | B | Current bill + Arrears + Total FPA = Payable within due date |
   | C | Tariff rate × units consumed = Cost of Electricity line |
   | D | FPA rate × units of the FPA's *reference month* (from the bill's own history table) = FPA line |

   Any failed identity caps confidence and marks the bill `needs_review`
   instead of silently showing numbers that might be wrong.
4. The API maps each raw charge label to a plain-language explanation,
   computes the bill's position in the tariff slab, and predicts next
   month's bill from its own 12-month usage history.
5. A second AI pass turns those computed facts into a personalized savings
   plan — concrete appliance habits paired with the rupee amount they'd
   save, generated from real numbers rather than invented ones.

## Architecture

```
┌─────────────────┐        /api/*        ┌──────────────────┐        ┌──────────────────┐
│  bijlisaver-web  │ ───────────────────► │  BijliSaver.Api   │ ─────► │  bijlisaver-ocr   │
│  React + Vite    │                      │  ASP.NET Core     │ ◄───── │  FastAPI + Gemini │
└─────────────────┘                      └────────┬──────────┘        └──────────────────┘
                                                    │
                                                    ▼
                                             ┌──────────────┐
                                             │  PostgreSQL   │
                                             │  (Neon)       │
                                             └──────────────┘
```

- **[bijlisaver-web](bijlisaver-web/README.md)** — the frontend. Upload
  screen with a live sample report, per-bill breakdown, and bill history
  with a usage chart.
- **[BijliSaver.Api](BijliSaver.Api/README.md)** — orchestrates the flow:
  calls the OCR service, maps raw charge labels to explanations, computes
  slab/tax/prediction insights, persists everything, and requests the AI
  savings plan.
- **[ocr-service/bijlisaver-ocr](ocr-service/bijlisaver-ocr/README.md)** —
  a single-purpose FastAPI service: bill image in, validated structured
  JSON out. Never trusts the vision model's output blindly.

## Tech stack

| Layer | Stack |
|---|---|
| Frontend | React, Vite, Tailwind CSS, React Router, Recharts |
| Backend | ASP.NET Core 10, Entity Framework Core, JWT auth |
| OCR / AI | FastAPI, Gemini (via an OpenAI-compatible endpoint) |
| Database | PostgreSQL (Neon in production) |
| Hosting | Render (API + OCR as Docker web services, frontend as a static site) |

## Project structure

```
BijliSaver/
├── bijlisaver-web/            React frontend
├── BijliSaver.Api/            ASP.NET Core API
│   └── Data/Migrations/       EF Core migrations (schema + seed data)
├── ocr-service/
│   └── bijlisaver-ocr/        FastAPI OCR + advice microservice
└── render.yaml                Render Blueprint (all three services)
```

## Local development

Each service has its own README with exact setup steps. Bring them up in
this order:

1. **PostgreSQL** — running locally, or point at any Postgres instance.
2. **[OCR service](ocr-service/bijlisaver-ocr/README.md)** — `uvicorn
   app.main:app --reload --port 8001`
3. **[API](BijliSaver.Api/README.md)** — `dotnet run` (applies EF Core
   migrations automatically on startup, including seed data for LESCO
   tariff slabs and charge definitions)
4. **[Frontend](bijlisaver-web/README.md)** — `npm run dev`, proxies `/api`
   to the local API in development

## Deployment

BijliSaver deploys entirely on free-tier infrastructure:

| Component | Provider | Why |
|---|---|---|
| Database | [Neon](https://neon.tech) | Free forever, standard Postgres wire protocol, no card required |
| API + OCR service | [Render](https://render.com) — free Docker web services | Sleep after 15 min idle, ~1 min cold start on wake — acceptable for low traffic |
| Frontend | Render — free Static Site | Served from a CDN, never sleeps |

Render's free tier has no private networking between services, so the API
and OCR service communicate over public HTTPS URLs, and the frontend calls
the API by its full public URL (`VITE_API_BASE_URL`) rather than a
same-origin proxy. CORS on the API is configuration-driven
(`AllowedOrigins__0`, `__1`, ...), so adding a custom domain later is a
config change, not a code change.

All three services and their build configuration are defined in a single
[`render.yaml`](render.yaml) Blueprint at the repo root — Render reads it
and provisions all three in one pass.

### Prerequisites

- A GitHub account with this repo pushed to it
- A [Neon](https://neon.tech) account (free)
- A [Render](https://render.com) account (free), connected to GitHub
- A Gemini API key from [Google AI Studio](https://aistudio.google.com/apikey)

### Step 1 — Create the Neon database

1. Sign up at [neon.tech](https://neon.tech) and create a project (a
   region close to your users minimizes latency — Singapore is closest to
   Pakistan).
2. Copy the connection string Neon gives you — it starts with
   `postgresql://` and includes the username, password, host, and
   database name in one string.

### Step 2 — Deploy the Render Blueprint

1. In the Render dashboard: **New → Blueprint**, then select this
   repository. Render detects `render.yaml` and proposes all three
   services (`bijlisaver-api`, `bijlisaver-ocr`, `bijlisaver-web`)
   automatically, with their build settings pre-filled.
2. When prompted for the secrets marked `sync: false` in `render.yaml`,
   provide:

   | Service | Variable | Value |
   |---|---|---|
   | `bijlisaver-api` | `DATABASE_URL` | The Neon connection string from Step 1 |
   | `bijlisaver-ocr` | `OPENAI_API_KEY` | Your Gemini API key |

   `Jwt__Key` is generated automatically by the blueprint — no action needed.
3. Click **Deploy Blueprint**. Render builds and deploys all three
   services; the first deploy takes a few minutes (Docker builds for the
   API and OCR service, `npm run build` for the frontend).

### Step 3 — Verify

Open the frontend's Render URL, upload a real bill, and confirm it returns
a full breakdown with a savings plan. If a service was asleep, the first
request after idle takes 30-60 seconds to wake it — this is expected
behavior on Render's free tier, not an error.

```bash
curl https://bijlisaver-api.onrender.com/api/bills
curl https://bijlisaver-ocr.onrender.com/health
```

### Database schema and seed data

The API applies EF Core migrations automatically on startup — no manual
SQL step is required. This includes:

- The full schema (bills, charges, insights, users, tariff slabs, ...)
- Reference data: DISCOs (LESCO, MEPCO, and three more Punjab DISCOs,
  currently inactive) and all 19 charge-definition codes the OCR
  extraction maps to
- LESCO tariff slab rates for FY 2025-26 (protected and non-protected),
  cross-checked against a real bill — slab warnings work for LESCO out of
  the box. Other DISCOs have no rate data seeded yet.

### Operational notes

- **Service names are global on Render.** If `bijlisaver-api`,
  `bijlisaver-ocr`, or `bijlisaver-web` are already taken, Render will ask
  you to rename them — update the cross-references in `render.yaml`
  (`OcrService__BaseUrl`, `AllowedOrigins__0`, `VITE_API_BASE_URL`) to
  match the actual assigned URLs before redeploying.
- **Free-tier limits**: 750 shared instance-hours/month across free
  Render web services (two sleeping services stay well under this at
  low traffic); Neon's free plan includes 0.5 GB storage, 100
  compute-hours/month, and 5 GB network transfer/month.
- Secrets (the Gemini key, JWT signing key, database credentials) live
  only in Render's environment variable store — nothing sensitive is
  committed to this repository.

## API overview

| Endpoint | Description |
|---|---|
| `POST /api/bills/upload` | Upload a bill photo/PDF → OCR → validate → map charges → compute insights → AI savings plan |
| `GET /api/bills` | List the signed-in user's bills |
| `GET /api/bills/{id}` | Full breakdown for one bill (owner or anonymous-upload only) |
| `DELETE /api/bills/{id}` | Delete a bill (owner only) |
| `POST /api/auth/register` / `POST /api/auth/login` | Account creation and JWT-based sign-in |

Bills can be uploaded without an account; only saving bill history and
viewing "My Bills" requires signing in.

## Known limitations

- OCR extraction and tariff-slab data are currently tuned for **LESCO**;
  other DISCOs' bill layouts are not yet supported even though the schema
  is DISCO-agnostic.
- Tariff slab rates should be verified against
  [nepra.org.pk](https://nepra.org.pk) before relying on them for
  high-stakes decisions — rates change periodically and secondary sources
  can disagree on upper-slab figures.
