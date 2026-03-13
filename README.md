# Job Command Center

A local-first LinkedIn job automation platform that attaches to your existing Chrome session via CDP ("God Mode") to safely harvest and manage job listings.

## Overview

Job Command Center prioritizes **account safety** by operating within your authenticated browser session rather than launching a fresh bot instance. It provides:

- **Stealth Harvesting**: Connects to Chrome via DevTools Protocol on port 9222
- **Dynamic Scoring**: Configurable weights for remote, contract, keywords, etc.
- **Pipeline Management**: Track jobs from discovery through application
- **Real-time Dashboard**: Blazor Server UI with live updates

## Quick Start

### Prerequisites

- .NET 10 SDK
- Chrome/Chromium browser
- Docker (for PostgreSQL)

### 1. Start Chrome with Remote Debugging

```bash
# macOS
/Applications/Google\ Chrome.app/Contents/MacOS/Google\ Chrome --remote-debugging-port=9222

# Windows
chrome.exe --remote-debugging-port=9222

# Linux
google-chrome --remote-debugging-port=9222
```

### 2. Run the Application

```bash
cd src/JobCommandCenter
dotnet run --project JobCommandCenter.AppHost
```

This will:
- Start PostgreSQL in a container
- Launch the Blazor Web UI
- Start the Harvester background worker

### 3. Open the Dashboard

Navigate to `http://localhost:5000` (or the URL shown in the console).

## Project Structure

```
src/JobCommandCenter/
├── JobCommandCenter.AppHost/          # Aspire orchestrator
├── JobCommandCenter.ServiceDefaults/  # OpenTelemetry, health checks
├── JobCommandCenter.Data/             # EF Core + PostgreSQL
├── JobCommandCenter.Shared/           # Domain models, scoring engine
├── JobCommandCenter.Harvester/        # Playwright CDP worker
├── JobCommandCenter.Web/              # Blazor Server UI
└── tests/
    ├── JobCommandCenter.UnitTests/
    └── JobCommandCenter.IntegrationTests/
```

## Technology Stack

- **.NET 10** with C# 12
- **.NET Aspire** for orchestration
- **Blazor Server** + **MudBlazor** for UI
- **PostgreSQL** + **Entity Framework Core**
- **Microsoft Playwright** for browser automation

## Key Constraints

1. **Harvester runs as host process** (not containerized) to access `localhost:9222`
2. **No credential handling** - authentication is via your existing Chrome session
3. **Human-mimicry delays** - randomized waits to avoid detection

## Development

```bash
# Build
dotnet build

# Run tests
dotnet test

# Run the AppHost (starts all services)
dotnet run --project JobCommandCenter.AppHost
```

## License

MIT
