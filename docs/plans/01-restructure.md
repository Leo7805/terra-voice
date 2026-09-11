# TerraVoice — Restructure

Goal: move to `src/` (backend) + `playground/` (renamed frontend) + `tests/` + root README. Backend and playground share one repo, deploy separately.

This file is the source of truth for the **outline**. Per `TEMPLATE_Common.md` "Learn-as-you-go Plan": implementation steps land in commits / PR descriptions, not in the plan. Detailed §1 and §7 plans were trimmed to keep this readable; the git history captures the removed detail.

---

## Outline

* [x] 1. Repo layout
  * [x] 1.1 Move backend into `src/TerraVoice.Api/`.
  * [x] 1.2 Rename `frontend/` → `playground/`.
  * [x] 1.3 Create `tests/` folder.
  * [x] 1.4 Add root `.editorconfig`.
* [ ] 2. Backend: code organization
  * [ ] 2.1 Reorganize by feature (Vertical Slice).
  * [ ] 2.2 Move config + options into `Common/`.
  * [ ] 2.3 Move typed error hierarchy into `Common/Errors/`.
  * [ ] 2.4 Move data access + external services behind `Infrastructure/` interfaces.
* [ ] 3. Backend: data layer
  * [ ] 3.1 Migrate raw SQLite to EF Core 10 (code-first).
  * [ ] 3.2 Define entity, DbContext, EF configuration.
  * [ ] 3.3 Repository interface in front of DbContext.
  * [ ] 3.4 Generate initial EF migration.
  * [ ] 3.5 Add Postgres provider for production; keep SQLite for dev/test.
  * [ ] 3.6 Postgres-based integration test fixture.
* [ ] 4. Backend: API surface
  * [ ] 4.1 Add API-key authentication handler.
  * [ ] 4.2 Require API key on sensitive endpoints.
  * [ ] 4.3 Add rate limiting.
  * [ ] 4.4 Fix SSML speaking-rate bug.
  * [ ] 4.5 Move exception-handler middleware to the front of the pipeline.
* [ ] 5. Backend: quality (tests + validation)
  * [ ] 5.1 Fail-fast options validation at boot.
  * [ ] 5.2 Add xUnit test project.
  * [ ] 5.3 Architecture tests for layering.
  * [ ] 5.4 Unit tests per feature handler.
  * [ ] 5.5 Minimal CI workflow.
* [ ] 6. Backend: observability
  * [ ] 6.1 Serilog for structured logging.
  * [ ] 6.2 OpenTelemetry tracing.
  * [ ] 6.3 `/health/live` + `/health/ready` endpoints.
  * [ ] 6.4 Application Insights exporter.
* [~] 7. Backend: deployment (in progress)
  * [x] 7.1 Dockerfile.
  * [x] 7.2 Docker Compose for VPS.
  * [x] 7.3 Environment variables + secrets (`.env.example`).
  * [x] 7.4 Persistent storage for SQLite (named volume, verified Sep 9).
  * [~] 7.5 GitHub Actions CI/CD.
    * [x] 7.5.1 Create `.github/workflows/backend.yml` (build + push GHCR on push/PR; deploy job on `workflow_dispatch`).
    * [x] 7.5.2 Commit + push; verify build + image push on the Actions tab.
    * [ ] 7.5.3 (After §7.9) Add VPS secrets (`VPS_HOST` / `VPS_USER` / `VPS_SSH_KEY`); verify manual deploy via `workflow_dispatch`.
  * [ ] 7.6 Caddy reverse proxy + HTTPS.
  * [x] 7.7 Container health checks (`HEALTHCHECK` + `/health/live`, `/health/ready`).
  * [ ] 7.9 Provision VPS (Docker + SSH + `ufw` + DNS A record).
  * [ ] 7.10 First manual deploy.
  * [ ] 7.11 Production smoke test.
* [ ] 8. Frontend: scaffold
  * [ ] 8.1 Finish `frontend/` → `playground/` tooling refs.
  * [ ] 8.2 Vite dev proxy (CORS-free local).
  * [ ] 8.3 Path aliases.
  * [ ] 8.4 Centralize React providers.
* [ ] 9. Frontend: structure (rebuild)
  * [ ] 9.1 Reorganize playground by feature.
  * [ ] 9.2 Replace hand-written API client with typed HTTP client.
  * [ ] 9.3 Replace tab UI with file-based routes.
  * [ ] 9.4 `app/` composition layer.
  * [ ] 9.5 Reorganize global styles.
* [ ] 10. Frontend: state and data
  * [ ] 10.1 TanStack Query for server state.
  * [ ] 10.2 Small global store for UI state.
  * [ ] 10.3 Form library + schema validation.
  * [ ] 10.4 Replace stats grid with usage chart.
  * [ ] 10.5 Network-level mocking for tests.
  * [ ] 10.6 Shared component library.
* [ ] 11. Frontend: testing
  * [ ] 11.1 Unit test runner + React Testing Library.
  * [ ] 11.2 E2E test runner.
  * [ ] 11.3 At least one unit test per feature hook.
* [ ] 12. Frontend: tooling
  * [ ] 12.1 Upgrade ESLint (type-aware).
  * [ ] 12.2 Add Prettier.
  * [ ] 12.3 Husky + lint-staged.
  * [ ] 12.4 Commitlint.
  * [ ] 12.5 Generate TS types from backend OpenAPI.
* [ ] 13. Frontend: deployment
  * [ ] 13.1 Dockerfile for playground.
  * [ ] 13.2 Bicep for Azure Static Web Apps.
  * [ ] 13.3 SWA proxy config for `/api`.
* [ ] 14. Documentation
  * [ ] 14.0 Project-level CLAUDE.md.
  * [ ] 14.1 Rewrite root README.
  * [ ] 14.2 Backend README.
  * [ ] 14.3 Playground README.

---

## §7 Backend deployment — key decisions

Target: own **Ubuntu VPS**, Docker + Caddy, SQLite stays (no Postgres).

| # | Decision |
|---|---|
| 7.1 | Multi-stage `sdk:10.0` → `aspnet:10.0`; `ASPNETCORE_URLS=http://+:8080`. |
| 7.2 | One `api` service, named volume `terravoice-data` at `/app/Data`, env via `.env`. |
| 7.3 | `.env.example` committed (template); `.env` gitignored at any depth. |
| 7.4 | `usage.db` at `/app/Data/usage.db`; named volume survives `down`/`up` (verified Sep 9). |
| 7.5 | GitHub Actions: build → push GHCR; manual deploy via `workflow_dispatch`. |
| 7.6 | Caddy auto-TLS via Let's Encrypt; forwards `https://<domain>` → `http://api:8080`. |
| 7.7 | Dockerfile `HEALTHCHECK` on `/health/live`; `/health/ready` runs `SELECT 1` against SQLite. |
| 7.9 | VPS: install Docker + SSH key + `ufw` open 22/80/443 + DNS A record. |
| 7.10 | Local `docker build` + `docker push ghcr.io/...` → VPS `docker compose pull && up -d`. |
| 7.11 | `curl https://<domain>/` and `/tts` from laptop. |

**Cut from scope (study project):** §7.8 rollback — single container, manual redeploy is enough.

**Execution order:** 7.9 → 7.10 → 7.6 → 7.11 → 7.5.

---

## Items deferred (study project scope)

Items not actively pursued for the current study project, kept here as reminders:

- §2 vertical-slice reorganization (codebase already follows it loosely).
- §3 EF Core migration (raw ADO.NET works for now).
- §4.1–4.3 API key / rate limiting (single client, study project).
- §5 test suite.
- §6 Serilog / OpenTelemetry / App Insights (Docker logs + health checks suffice).
- §8–13 frontend rebuild (playground still works on the old layout).