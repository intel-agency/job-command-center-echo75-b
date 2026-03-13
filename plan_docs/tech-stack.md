# Job Command Center — Tech Stack

## Runtime & Language
- .NET 10.0
- C# 12

## Architecture & Orchestration
- .NET Aspire (AppHost orchestration, service wiring)
- ServiceDefaults (OpenTelemetry, health checks)

## Automation
- Microsoft.Playwright (ConnectOverCDPAsync to existing Chrome)

## UI
- Blazor Server
- MudBlazor (UI components: grids, Kanban, dashboards)

## Data
- PostgreSQL
- Entity Framework Core (Npgsql provider)

## Observability & Logging
- OpenTelemetry (tracing/metrics)
- Serilog (structured logging)

## Containerization / Hosting
- Aspire-managed Postgres container
- Web project container-ready
- **Harvester must run as a host process (NOT containerized)**

## Dev Tooling
- dotnet SDK 10
- EF Core migrations tooling
- Playwright browser install scripts (per .NET Playwright docs)

## Testing
- Unit tests: xUnit
- Integration tests: EF Core + PostgreSQL
- E2E tests: Playwright

## Key Constraints
- CDP attach to user's existing Chrome on port 9222
- No LinkedIn credential handling
- Stealth-first behavior (human mimicry delays)
