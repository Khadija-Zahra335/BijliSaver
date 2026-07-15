# BijliSaver API (ASP.NET Core backend)

Receives bill uploads from the React frontend, calls the Python OCR service,
maps charges to plain-language explanations, stores everything in PostgreSQL,
and computes insights (slab position, tax %, savings tips).

## Prerequisites
1. .NET 8 SDK  (check: `dotnet --version`)
2. PostgreSQL running locally, with a database named `bijlisaver`
   and `schema_v2.sql` executed on it
3. The Python OCR service running on port 8001

## Setup
1. Put your PostgreSQL password in `appsettings.json` → ConnectionStrings
2. `dotnet restore`
3. `dotnet run`
4. Open the Swagger page it prints (e.g. http://localhost:5000/swagger)

## Flow
POST /api/bills/upload  → OCR → map → save → insights → full breakdown JSON
GET  /api/bills          → list for the history dashboard
GET  /api/bills/{id}     → full breakdown for the detail screen

## Notes
- Tariff slabs must be seeded in `tariff_slabs` before slab insights appear
  (see schema_v2.sql note; rates from your own bill + nepra.org.pk).
- `needs_review` bills are stored but skipped for insights.
- Tax percentage is always computed against current_bill, never payable
  (arrears would otherwise distort it — verified on a real Rs 64k-arrears bill).
