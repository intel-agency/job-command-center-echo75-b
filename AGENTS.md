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

## Logging Conventions

### Overview

This project uses **Serilog** for structured logging with automatic enrichment for distributed tracing, job context, and service identity. All logs are structured with named properties for easy querying and analysis.

### Structured Message Templates

#### Naming Conventions
- **Property names**: Use PascalCase (e.g., `JobId`, `CorrelationId`, `PageCount`)
- **Message templates**: Use named placeholders, not positional or interpolated strings
- **Enricher properties**: Automatically added via `JobContextEnricher`

#### Good vs Bad Examples

```csharp
// ✅ GOOD: Named placeholders with structured properties
_logger.LogInformation("Processing job {JobId} with status {Status}", jobId, status);

// ✅ GOOD: Properties are queryable in log aggregators
_logger.LogWarning("Harvest operation took {Duration}ms, exceeding threshold of {Threshold}ms", 
    duration, threshold);

// ❌ BAD: String interpolation loses structure
_logger.LogInformation($"Processing job {jobId}");

// ❌ BAD: Positional parameters without names
_logger.LogInformation("Processing job {0}", jobId);

// ❌ BAD: Concatenated strings
_logger.LogInformation("Processing job " + jobId);
```

### Log Levels

| Level | When to Use | Examples |
|-------|-------------|----------|
| **Verbose/Trace** | Detailed diagnostic info for debugging | Method entry/exit, variable dumps, internal state |
| **Debug** | Development-time information | SQL queries, API payloads, detailed flow |
| **Information** | General operational flow | Service start/stop, job processed, request completed |
| **Warning** | Unexpected but handled situations | Retry attempts, degraded performance, missing optional data |
| **Error** | Failures that don't stop the application | Failed API calls, parsing errors, unprocessable items |
| **Fatal** | Critical failures requiring app shutdown | Database connection failed, configuration invalid |

#### Log Level Examples

```csharp
// Verbose/Trace - Very detailed, typically disabled in production
_logger.LogTrace("Entering method {MethodName} with parameters {@Parameters}", 
    nameof(ProcessJob), new { jobId, status });

// Debug - Development troubleshooting
_logger.LogDebug("SQL query executed: {Query} in {Duration}ms", query, duration);

// Information - Normal operations
_logger.LogInformation("Job {JobId} processed successfully in {Duration}ms", jobId, duration);
_logger.LogInformation("Harvester connected to Chrome on port {Port}", port);

// Warning - Unexpected but recoverable
_logger.LogWarning("Rate limit reached, waiting {RetryAfter}s before retry", retryAfter);
_logger.LogWarning("Job {JobId} has missing optional field {FieldName}", jobId, fieldName);

// Error - Operation failed but app continues
_logger.LogError(ex, "Failed to parse job {JobId} from page content", jobId);
_logger.LogError("HTTP request to {Url} failed with status {StatusCode}", url, statusCode);

// Fatal - Application cannot continue
_logger.LogFatal(ex, "Critical database connection failed, shutting down");
_logger.LogFatal("Required configuration {ConfigKey} is missing", configKey);
```

### Serilog Configuration

#### Configuration in appsettings.json

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning",
        "Microsoft.Hosting.Lifetime": "Information"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "theme": "Ansi",
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}    └─ {SourceContext}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/jcc-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7,
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
          "shared": true
        }
      }
    ]
  }
}
```

#### Program.cs Setup

```csharp
// Create bootstrap logger for early startup errors
Log.Logger = SerilogConfiguration.CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    
    // Configure full Serilog from appsettings.json
    builder.Services.AddSerilog((services, loggerConfiguration) =>
    {
        var config = services.GetRequiredService<IConfiguration>();
        var logger = SerilogConfiguration.ConfigureSerilog(
            config, 
            applicationName: "JobCommandCenter.Harvester",
            environmentName: builder.Environment.EnvironmentName);
        loggerConfiguration.WriteTo.Logger(logger);
    });
    
    var app = builder.Build();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
```

### Enrichment Patterns

#### Automatic Enrichment

The `JobContextEnricher` automatically adds these properties to all logs:
- `JobId` - Current job being processed
- `CorrelationId` - Request/operation correlation ID
- `OperationId` - Unique operation identifier
- `TraceId` / `SpanId` - OpenTelemetry distributed tracing
- `RequestPath` / `HttpMethod` - HTTP context (when available)

#### Using Context Scopes

```csharp
public class JobProcessor
{
    private readonly ILogger<JobProcessor> _logger;
    
    public async Task ProcessJobAsync(Job job)
    {
        // All logs within this scope automatically include JobId and CorrelationId
        using var scope = JobContextEnricher.BeginJobScope(job.Id, job.CorrelationId);
        
        _logger.LogInformation("Starting job processing");
        
        // Nested operation scope
        using var opScope = JobContextEnricher.BeginOperationScope();
        await DoWorkAsync(job);
        
        _logger.LogInformation("Job completed");
    }
}
```

### OpenTelemetry Integration

Logs are automatically correlated with traces when OpenTelemetry is configured:

```json
{
  "OTEL_EXPORTER_OTLP_ENDPOINT": "http://localhost:4317",
  "OTEL_SERVICE_NAME": "JobCommandCenter.Harvester"
}
```

Enriched properties include:
- `TraceId` - Links log to distributed trace
- `SpanId` - Current span within trace
- `ServiceName` / `ServiceVersion` - Service identity

### Metrics Integration

Use `HarvesterMetrics` for operational metrics alongside logging:

```csharp
public class HarvestWorker : BackgroundService
{
    private readonly HarvesterMetrics _metrics;
    private readonly ILogger<HarvestWorker> _logger;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        using var connectionScope = _metrics.BeginConnectionScope();
        
        try
        {
            var jobs = await HarvestJobsAsync(stoppingToken);
            stopwatch.Stop();
            
            // Record both metrics and structured logs
            _metrics.RecordJobsHarvested(jobs.Count, source: "search", status: "new");
            _metrics.RecordHarvestDuration(stopwatch.Elapsed, success: true);
            
            _logger.LogInformation("Harvested {Count} jobs in {Duration}ms", 
                jobs.Count, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _metrics.RecordHarvestDuration(stopwatch.Elapsed, success: false);
            _metrics.RecordError("harvest_failed", operation: "full_cycle");
            
            _logger.LogError(ex, "Harvest operation failed");
        }
    }
}
```

### Best Practices

1. **Always use structured logging** - Named placeholders, not string interpolation
2. **Use appropriate log levels** - Don't log everything as Information
3. **Include context via scopes** - Let enrichers add JobId, CorrelationId
4. **Log exceptions properly** - Pass exception as first parameter
5. **Use semantic property names** - `Duration` not `Ms`, `Count` not `Num`
6. **Avoid sensitive data** - Never log credentials, tokens, or PII

### Log Output Example

```
[14:32:15 INF] Starting harvest cycle
    └─ JobCommandCenter.Harvester.Workers.HarvestWorker
  JobId: "abc123"
  CorrelationId: "550e8400-e29b-41d4-a716-446655440000"
  TraceId: "4bf92f3577b34da6a3ce929d0e0e4736"

[14:32:17 INF] Harvested 25 jobs in 2341ms
    └─ JobCommandCenter.Harvester.Workers.HarvestWorker
  JobId: "abc123"
  OperationId: "6ba7b810-9dad-11d1-80b4-00c04fd430c8"
```
