# TerraVoice

A lightweight **Text-to-Speech (TTS)** studio. Type text, pick a voice, get audio.

The backend is a **.NET 10 Web API** backed by Azure Speech, with usage logging in SQLite. The `playground/` folder is a small React + Vite + MUI demo client used as a test consumer — not the product.

> **Status:** early-stage scaffold. The backend runs locally and ships as a Docker image; deployment targets a self-hosted VPS via GitHub Actions. See [docs/plans/](docs/plans/) for the full plan.

---

## Tech Stack

### Backend — [`src/TerraVoice.Api/`](src/TerraVoice.Api/)

- ASP.NET Core (.NET 10) — Minimal API
- Azure Speech SDK (`Microsoft.CognitiveServices.Speech`)
- SQLite via `Microsoft.Data.Sqlite` (usage logging)
- Swagger UI (`Swashbuckle.AspNetCore`)
- Container: multi-stage Dockerfile, runs on `aspnet:10.0`

### Playground — [`playground/`](playground/)

- React 19 + TypeScript
- Vite
- MUI
- `<audio>` element for playback

---

## Repository Layout

```
terra-voice/
├── src/
│   └── TerraVoice.Api/      # Backend (the product)
│       ├── Configuration/   # Options bound from appsettings
│       ├── Data/            # SQLite schema + UsageDb
│       ├── Dtos/            # Request/response shapes
│       ├── Endpoints/       # Minimal API: /tts, /usage, /health/*
│       ├── Errors/          # Typed exception hierarchy
│       ├── Extensions/      # DI / pipeline extensions
│       ├── Services/        # TtsService, UsageService
│       ├── Program.cs
│       ├── appsettings.json
│       └── Dockerfile       # Multi-stage, used by docker-compose
├── playground/              # React demo client (test consumer)
│   ├── components/
│   ├── services/
│   ├── src/
│   ├── index.html
│   └── package.json
├── tests/                   # Placeholder for future test projects
├── docs/
│   └── plans/               # Restructure plan + roadmap
├── docker-compose.yml       # Backend container (local + VPS)
├── .env.example             # Template for runtime secrets (.env is gitignored)
├── .editorconfig
├── .dockerignore
├── TerraVoice.sln
└── README.md
```

---

## Quick Start — Docker (recommended)

```sh
cp .env.example .env                 # then edit .env with your Azure Speech key + region
docker compose up --build            # → API on http://localhost:8080
```

The compose file builds [`src/TerraVoice.Api/Dockerfile`](src/TerraVoice.Api/Dockerfile) from the repo root, mounts a named volume at `/app/Data` so the SQLite file survives restarts, and reads secrets from `.env`.

Optionally, in another shell:

```sh
cd playground
npm install
npm run dev                          # → http://localhost:5173
```

---

## Quick Start — without Docker

### Backend

```sh
cd src/TerraVoice.Api
dotnet restore
dotnet run
```

Default: `http://localhost:5059` (see `Properties/launchSettings.json`).
Swagger UI: `http://localhost:5059/swagger`.

Pass Azure Speech credentials via environment variables (no user-secrets step):

```sh
AzureSpeech__Key=YOUR_KEY AzureSpeech__Region=australiaeast dotnet run
```

### Playground

```sh
cd playground
npm install
npm run dev                          # → http://localhost:5173
```

---

## Configuration

All defaults live in [`src/TerraVoice.Api/appsettings.json`](src/TerraVoice.Api/appsettings.json). Anything can be overridden via environment variables using **double-underscore** notation (`:` becomes `__`, arrays use `__0`, `__1`, …).

### Required secrets

| Key                       | Description                                            |
| ------------------------- | ------------------------------------------------------ |
| `AzureSpeech__Key`        | Azure Speech subscription key                          |
| `AzureSpeech__Region`     | Region of your Azure Speech resource (default `australiaeast`) |

See [`.env.example`](.env.example) for the full template.

### Tunables (defaults from `appsettings.json`)

| Key                                   | Default                  |
| ------------------------------------ | ------------------------ |
| `Tts__MaxTextLength`                 | `3000`                   |
| `Tts__DefaultEnglishVoiceName`       | `en-US-JennyNeural`      |
| `Tts__DefaultChineseVoiceName`       | `zh-CN-XiaoxiaoNeural`   |
| `Tts__DefaultSpeakingRate`           | `1.0`                    |
| `Usage__MonthlyCharQuota`            | `500000`                 |
| `Usage__Provider`                    | `azure`                  |
| `ConnectionStrings__UsageDb`         | `Data Source=Data/usage.db` |

### CORS

Allowed origins live in `Cors:AllowedOrigins` (see `appsettings.json`). For local dev include `http://localhost:5173` and `http://localhost:5174`. For production, replace with your real domain — set `Cors__AllowedOrigins__N` env vars on the VPS.

---

## API Contract

Base URL: `http://localhost:5059` (local) or `http://localhost:8080` (Docker).

### `POST /tts`

Synthesize audio from text. Caching is disabled (`Cache-Control: no-store`).

Request:

```json
{
  "text": "Hello 你好",
  "voiceName": "en-US-JennyNeural"
}
```

Response: `200 OK` with `audio/mpeg` body. `400` / `500` for invalid input or upstream failures.

### `GET /usage`

Returns current-month aggregated usage stats (request count, character count).

### `GET /health/live`

Liveness probe — process is up. Never depends on DB or external services.

### `GET /health/ready`

Readiness probe — `200` if SQLite is reachable, `503` otherwise.

---

## Voice Options

Grouped by region (Azure Neural voices):

- US English — `en-US-JennyNeural`, etc.
- UK English
- Australian English
- Chinese (Mandarin) — `zh-CN-XiaoxiaoNeural`, etc.
- Mixed Chinese/English — use bilingual text

---

## Deployment

The backend ships as a single Docker image to a self-hosted **Ubuntu VPS** via [`.github/workflows/backend.yml`](.github/workflows/backend.yml):

```
push to main → CI builds + tests + pushes image to GHCR → SSH to VPS → docker compose pull && up -d
```

Persistent SQLite data lives on a named volume (`terravoice-data`) mounted at `/app/Data`. Reverse proxy + TLS are planned separately. Full plan: [docs/plans/01-restructure.md §7](docs/plans/01-restructure.md).

The playground is a demo consumer — it deploys independently (typically Vercel) and is out of scope for this repo's deployment pipeline.

---

## Plans & Roadmap

- [docs/plans/01-restructure.md](docs/plans/01-restructure.md) — full repo restructure outline + detailed plans for completed sections
- [docs/plans/roadmap.md](docs/plans/roadmap.md) — one-line status of each section

Sections completed: §1 (repo layout), §7.1–§7.3 (Dockerfile, Compose, secrets).