# AGENTS.md

## Project Overview

**Job Command Center** is a local-first LinkedIn job automation platform that attaches to an existing Chrome session via Chrome DevTools Protocol (CDP). It prioritizes account safety by operating within an authenticated browser context rather than launching a fresh bot instance.

**Key Principle**: The Harvester must run as a **host process** (not containerized) to access `localhost:9222`.

## Setup Commands

```bash
# Install dependencies
cd src/JobCommandCenter && dotnet restore

# Build
dotnet build

# Run tests
dotnet test

# Run the application (starts all services via Aspire)
dotnet run --project JobCommandCenter.AppHost
```

**Prerequisite**: Start Chrome with remote debugging before running the app:

```bash
# macOS
/Applications/Google\ Chrome.app/Contents/MacOS/Google\ Chrome --remote-debugging-port=9222

# Linux
google-chrome --remote-debugging-port=9222

# Windows
chrome.exe --remote-debugging-port=9222
```

## Project Structure

```
src/JobCommandCenter/
├── JobCommandCenter.slnx              # Solution file (slnx format)
├── JobCommandCenter.AppHost/          # Aspire orchestrator
│   └── AppHost.cs                     # Configures Postgres + services
├── JobCommandCenter.ServiceDefaults/  # OpenTelemetry, health checks
├── JobCommandCenter.Data/             # EF Core DbContext
│   ├── AppDbContext.cs
│   └── Entities/JobEntity.cs
├── JobCommandCenter.Shared/           # Domain models
│   ├── Models/Job.cs
│   ├── Models/JobStatus.cs
│   ├── Models/ScoringConfig.cs
│   └── Services/ScoringEngine.cs
├── JobCommandCenter.Harvester/        # Playwright CDP worker
│   ├── Program.cs
│   └── Workers/HarvestWorker.cs
├── JobCommandCenter.Web/              # Blazor Server UI
│   └── Components/
└── tests/
    ├── JobCommandCenter.UnitTests/
    └── JobCommandCenter.IntegrationTests/
```

## Code Style

- **Language**: C# 12 with .NET 10
- **Nullable**: Enabled (`#nullable enable`)
- **Warnings**: Treat as errors in release builds
- **Naming**: Follow Microsoft C# naming conventions
- **Async**: Use `async`/`await`; avoid `.Result` or `.Wait()`

## Testing Instructions

- Run all tests: `dotnet test`
- Run specific test project: `dotnet test tests/JobCommandCenter.UnitTests`
- Integration tests require Docker (PostgreSQL container)

## Architecture Notes

### Service Architecture
1. **AppHost** - Aspire orchestrator that manages PostgreSQL container and launches services
2. **Harvester** - Background worker that connects to Chrome via CDP on port 9222
3. **Web** - Blazor Server dashboard with MudBlazor components

### Data Flow
1. User launches Chrome with `--remote-debugging-port=9222`
2. Harvester connects via CDP and scrapes LinkedIn job listings
3. Jobs are persisted to PostgreSQL via EF Core
4. Web UI displays jobs in real-time via Blazor Server

### Critical Constraints
- **Harvester must NOT be containerized** - it needs access to host's `localhost:9222`
- **No credential handling** - authentication piggybacks on user's Chrome session
- **Human-mimicry delays** - randomized waits to avoid bot detection

## PR and Commit Guidelines

- Commit message format: `type(scope): message`
- Types: `feat`, `fix`, `docs`, `test`, `refactor`, `chore`
- PRs should target `main` branch
- Run `dotnet build` and `dotnet test` before committing

## Common Pitfalls

1. **Chrome not running with debug port**: Harvester will fail to connect. Always start Chrome first.
2. **Containerized Harvester**: Will fail to reach `localhost:9222`. Must run as host process.
3. **Missing Playwright browsers**: Run `pwsh bin/Debug/net10.0/playwright.ps1 install` after first build

## CI/CD

- Validation workflow runs on PR
- Build, test, and lint checks
- Docker image published to GHCR
