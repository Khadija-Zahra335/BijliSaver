# BijliSaver Web (React frontend)

The Saada-themed frontend. Three screens:
- Upload (/) — drag-and-drop bill upload with honest progress stages
- Bill detail (/bills/:id) — the signature "every charge, explained" breakdown
- My Bills (/history) — list + units-per-month chart

## Prerequisites
- Node.js 18+  (check: `node --version`; install from nodejs.org if missing)
- The .NET backend running on http://localhost:5000
- The Python OCR service running on http://localhost:8001

## Run
```
npm install
npm run dev
```
Open the URL it prints (usually http://localhost:5173).

Vite proxies /api → localhost:5000, so no CORS issues in development.
If your backend runs on a different port, edit vite.config.js.
