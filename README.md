# TerraVoice

A lightweight **Text‑to‑Speech (TTS)** studio built with **.NET 10 Web API** and **React + Vite + MUI**.  
Provides a simple UI for entering text, selecting a voice, and generating audio using **Azure Speech Service**.

---

## Tech Stack

### Backend (.NET 10)

- ASP.NET Core Web API
- Azure Speech SDK (`Microsoft.CognitiveServices.Speech`)
- Minimal API endpoints (`POST /api/tts`, `GET /api/usage`)
- CORS enabled for local development

- NuGet Packages
  - `Swashbuckle.AspNetCore`
  - `Microsoft.CognitiveServices.Speech`
  - `Microsoft.Data.Sqlite`

### Frontend (React)

- React 19 + TypeScript
- Vite
- MUI
- Fetch-based API client
- Audio playback via `<audio>` element

- Node.js Packages
  - `@mui/material`, `@emotion/react`, `@emotion/styled`, `@mui/icons-material`

---

## Directory Structure

```
terra-voice/
├── backend/
│   └── TerraVoice/
│       ├── TerraVoice.sln
│       └── TerraVoice.Api/
│           ├── Configuration/   # Options bound from appsettings
│           ├── Data/             # SQLite schema + UsageDb
│           ├── Dtos/             # Request/response shapes
│           ├── Endpoints/        # Minimal API endpoint groups
│           ├── Errors/           # Typed exception hierarchy
│           ├── Extensions/       # DI / pipeline extensions
│           ├── Services/         # TtsService, UsageService
│           ├── _docs/            # Internal notes
│           ├── Program.cs
│           └── appsettings.json
└── frontend/
    ├── components/
    ├── services/
    ├── src/
    ├── index.html
    └── package.json
```

---

## Backend Setup

### Required NuGet Packages

- `Microsoft.CognitiveServices.Speech` (Azure Speech TTS SDK)
- `Swashbuckle.AspNetCore` (Swagger UI)
- `Microsoft.Data.Sqlite` (usage logging)

### Configuration

Add your Azure Speech credentials. The recommended path is the **.NET user-secrets store**:

```shell
cd backend/TerraVoice/TerraVoice.Api
dotnet user-secrets set "AzureSpeech:SubscriptionKey" "YOUR_KEY"
dotnet user-secrets set "AzureSpeech:Region" "YOUR_REGION"
```

> The project ships with a default `appsettings.json` and an empty `appsettings.Development.json`.  
> `ConnectionStrings:Default` points at `Data/terra-voice.db` and `ConnectionStrings:UsageDb` at `Data/usage.db` — both are auto-created on first run from `Data/`.

### Run Backend

```shell
cd backend/TerraVoice/TerraVoice.Api
dotnet restore
dotnet run
```

Default: `http://localhost:5059` (see `Properties/launchSettings.json`).  
Swagger UI: `http://localhost:5059/swagger`.

---

## Frontend Setup

### Install & Run

```sh
cd frontend
npm install
npm run dev
```

Default: `http://localhost:5173`

### Voice Options

Grouped by region:

- US English
- UK English
- Australian English
- Chinese (Mandarin)
- Mixed Chinese/English (for bilingual text)

---

## API Contract

### POST /tts

Request:

```json
{
  "text": "Hello 你好",
  "voiceName": "en-US-JennyNeural"
}
```

Response:

- `200 OK` — audio stream (`audio/mpeg`)
- `400/500` — error message

### GET /usage

Returns aggregated usage stats (current month, characters consumed, etc.).

---

## Deployment Recommendation

- **Frontend:** Azure Static Web Apps (SWA) or Vercel
- **Backend:** Azure App Service (.NET 10)
- **Proxy:** SWA API proxy → App Service `/api/*`

This provides a clean, modern, low‑latency architecture.
