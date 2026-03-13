# Job Command Center — Architecture

## Overview
Job Command Center is a local-first LinkedIn job automation platform that attaches to an existing, authenticated Chrome session via CDP ("God Mode"). The system prioritizes stealth, data ownership, and real-time visibility.

## Architectural Goals
- Stealth-first automation using existing browser fingerprint/session
- Local data sovereignty (PostgreSQL)
- Unified orchestration via .NET Aspire
- Shared domain models across Harvester and UI

## High-Level Diagram (Mermaid)
```mermaid
graph TD
  User[User's Local Chrome] <-->|CDP Port 9222| Harvester
  subgraph AspireHost[.NET Aspire AppHost]
    Harvester[JobCommandCenter.Harvester]
    Web[JobCommandCenter.Web (Blazor Server)]
    Postgres[(PostgreSQL)]
  end
  Harvester -->|Writes Jobs| Postgres
  Web -->|Reads/Updates Jobs| Postgres
  Web -->|Configures| Harvester
```

## Core Components
- **JobCommandCenter.AppHost**: Orchestrates services, injects connection strings/env.
- **JobCommandCenter.ServiceDefaults**: OpenTelemetry, health checks, shared defaults.
- **JobCommandCenter.Data**: EF Core DbContext, migrations, PostgreSQL access.
- **JobCommandCenter.Shared**: Domain models (Job, JobStatus, ScoringConfig).
- **JobCommandCenter.Harvester**: Playwright worker; must run as host process.
- **JobCommandCenter.Web**: Blazor Server UI (MudBlazor dashboard).

## Key Data Flows
1. User launches Chrome with `--remote-debugging-port=9222`
2. Harvester attaches via CDP and scrapes job listings
3. Harvester writes jobs + audit history to PostgreSQL
4. Web UI reads/updates job pipeline and scoring settings

## Security & Privacy
- No LinkedIn credential handling
- Local DB storage; production uses secure env vars
- Harvester logging is structured and avoids secrets

## Deployment Notes
- Postgres via Aspire container
- Web can be containerized
- Harvester must remain a host process for CDP loopback access
