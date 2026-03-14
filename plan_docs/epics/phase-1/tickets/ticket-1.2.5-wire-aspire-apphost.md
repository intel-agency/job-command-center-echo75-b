# Story: Wire AppDbContext into Aspire (Epic 1.2: #5)

## Ticket Metadata

| Field | Value |
|-------|-------|
| **Ticket ID** | 1.2.5 |
| **Epic** | 1.2 - Data Modeling |
| **Story Points** | 2 |
| **Priority** | High |
| **Status** | Ready |
| **Assignee** | TBD |
| **Sprint** | Phase 1 |

---

## Objective

Wire the `AppDbContext` into the .NET Aspire orchestration layer, configuring the AppHost to provision PostgreSQL and inject the database connection into both the Web and Harvester services. Additionally, implement automatic migration application on service startup.

---

## Scope

**Files Affected:**
- `src/JobCommandCenter/JobCommandCenter.AppHost/AppHost.cs` (verify/update)
- `src/JobCommandCenter/JobCommandCenter.Web/Program.cs` (update for auto-migration)
- `src/JobCommandCenter/JobCommandCenter.Harvester/Program.cs` (update for auto-migration)
- `src/JobCommandCenter/JobCommandCenter.ServiceDefaults/Extensions.cs` (potential update for DB health checks)

**Out of Scope:**
- Production deployment configuration
- Multi-environment configuration (dev/staging/prod)

---

## Context

.NET Aspire provides a unified orchestration model for cloud-native applications. The AppHost is responsible for:

1. **Provisioning Infrastructure:** Creating PostgreSQL containers and databases
2. **Service Discovery:** Injecting connection strings and service endpoints
3. **Lifecycle Management:** Starting services in the correct order

This ticket ensures that when the Aspire AppHost starts:
1. A PostgreSQL container is launched
2. The `jobdb` database is created
3. The Web and Harvester services receive the connection string
4. Migrations are applied automatically

---

## Technical Specifications

### AppHost Configuration

The AppHost should configure PostgreSQL and wire it to the services:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Add PostgreSQL with database
var postgres = builder.AddPostgres("postgres");
var db = postgres.AddDatabase("jobdb");

// Wire Web service to database
builder.AddProject<JobCommandCenter_Web>("web")
    .WithReference(db);

// Wire Harvester service to database
builder.AddProject<JobCommandCenter_Harvester>("harvester")
    .WithReference(db);

builder.Build().Run();
```

### Service Configuration

Each service should:

1. **Use Aspire's Npgsql Integration:**
   ```csharp
   builder.AddNpgsqlDbContext<AppDbContext>("jobdb");
   ```

2. **Apply Migrations on Startup:**
   ```csharp
   // In Web/Program.cs or a startup filter
   app.Services.GetRequiredService<AppDbContext>().Database.MigrateAsync();
   ```

### Connection String Injection

Aspire automatically injects the connection string as:
- Environment variable: `ConnectionStrings__jobdb`
- Accessible via: `builder.Configuration.GetConnectionString("jobdb")`

---

## Plan

### Part A: Verify/Update AppHost Configuration

- [ ] **Step 1: Open AppHost.cs**
  - Navigate to `src/JobCommandCenter/JobCommandCenter.AppHost/AppHost.cs`

- [ ] **Step 2: Verify PostgreSQL configuration**
  - Ensure `AddPostgres("postgres")` is called
  - Ensure `AddDatabase("jobdb")` is called on the postgres builder

- [ ] **Step 3: Verify service references**
  - Ensure Web project has `.WithReference(db)`
  - Ensure Harvester project has `.WithReference(db)`

- [ ] **Step 4: Update if necessary**
  - Add missing configurations following the pattern above

### Part B: Update Web Project for Auto-Migration

- [ ] **Step 5: Open Web/Program.cs**
  - Navigate to `src/JobCommandCenter/JobCommandCenter.Web/Program.cs`

- [ ] **Step 6: Verify/Update database context registration**
  - Current: `builder.Services.AddDbContext<AppDbContext>(...)`
  - Consider using: `builder.AddNpgsqlDbContext<AppDbContext>("jobdb")` (Aspire integration)

- [ ] **Step 7: Add auto-migration on startup**
  ```csharp
  // After var app = builder.Build();
  
  // Apply migrations automatically in development
  if (app.Environment.IsDevelopment())
  {
      using var scope = app.Services.CreateScope();
      var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
      dbContext.Database.MigrateAsync().GetAwaiter().GetResult();
  }
  ```

- [ ] **Step 8: Add database health check (optional)**
  ```csharp
  builder.Services.AddHealthChecks()
      .AddNpgSql(builder.Configuration.GetConnectionString("jobdb")!);
  ```

### Part C: Update Harvester Project for Auto-Migration

- [ ] **Step 9: Open Harvester/Program.cs**
  - Navigate to `src/JobCommandCenter/JobCommandCenter.Harvester/Program.cs`

- [ ] **Step 10: Verify/Update database context registration**
  - Similar to Web project

- [ ] **Step 11: Add auto-migration on startup**
  ```csharp
  // After var host = builder.Build();
  
  // Apply migrations automatically
  using (var scope = host.Services.CreateScope())
  {
      var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
      await dbContext.Database.MigrateAsync();
  }
  
  await host.RunAsync();
  ```

### Part D: Test End-to-End

- [ ] **Step 12: Ensure Docker is running**
  - Aspire requires Docker for PostgreSQL container

- [ ] **Step 13: Run the AppHost**
  ```bash
  cd src/JobCommandCenter/JobCommandCenter.AppHost
  dotnet run
  ```

- [ ] **Step 14: Verify Aspire Dashboard**
  - Open the Aspire dashboard (usually at `http://localhost:15000` or printed in console)
  - Verify PostgreSQL resource is running
  - Verify Web service is running and healthy
  - Verify Harvester service is running

- [ ] **Step 15: Verify database connectivity**
  - Check Web service logs for successful migration
  - Navigate to Web UI and verify no database errors
  - Check PostgreSQL container logs

- [ ] **Step 16: Verify migrations applied**
  - Connect to PostgreSQL container
  - Verify Jobs and HistoryLogs tables exist
  ```bash
  docker exec -it <container-id> psql -U postgres -d jobdb -c "\dt"
  ```

---

## Acceptance Criteria

| # | Criterion | Verification |
|---|-----------|--------------|
| 1 | AppHost configures PostgreSQL with `AddPostgres("postgres")` | Code review |
| 2 | AppHost creates database with `AddDatabase("jobdb")` | Code review |
| 3 | Web project references database with `.WithReference(db)` | Code review |
| 4 | Harvester project references database with `.WithReference(db)` | Code review |
| 5 | Web service registers AppDbContext correctly | Code review |
| 6 | Harvester service registers AppDbContext correctly | Code review |
| 7 | Migrations apply automatically on Web startup | Log verification |
| 8 | Migrations apply automatically on Harvester startup | Log verification |
| 9 | `dotnet run --project JobCommandCenter.AppHost` succeeds | Command execution |
| 10 | Aspire dashboard shows PostgreSQL resource as healthy | Dashboard verification |
| 11 | Aspire dashboard shows Web service as healthy | Dashboard verification |
| 12 | Aspire dashboard shows Harvester service as healthy | Dashboard verification |
| 13 | Database tables are created (Jobs, HistoryLogs) | SQL verification |
| 14 | No connection string errors in service logs | Log review |
| 15 | `dotnet build` succeeds with no errors | CI/CD |

---

## Validation Commands

```bash
# Build the solution
cd src/JobCommandCenter
dotnet build

# Run the AppHost (requires Docker)
cd JobCommandCenter.AppHost
dotnet run

# Verify services are healthy (check Aspire dashboard URL from console output)
# Usually: http://localhost:15000 or similar

# Connect to PostgreSQL container to verify schema
docker ps  # Find the postgres container ID
docker exec -it <container-id> psql -U postgres -d jobdb -c "\dt"
docker exec -it <container-id> psql -U postgres -d jobdb -c "\d \"Jobs\""

# Stop the AppHost (Ctrl+C)

# Clean up Docker resources (optional)
docker system prune -f
```

### Verifying Database Connection

```bash
# From within the Web container or via psql
# Check that migrations were applied
SELECT * FROM "__EFMigrationsHistory";

# Check table structure
\d "Jobs"
\d "HistoryLogs"

# Check indexes
\di
```

---

## Dependencies

| Dependency | Type | Status |
|------------|------|--------|
| Ticket 1.2.4 | Initial migration | Required |
| Docker Desktop | Infrastructure | Required |
| .NET Aspire Workload | SDK | Required |

This ticket depends on the initial migration being generated in ticket 1.2.4.

---

## Test Strategy

### Integration Tests

Create integration tests in `tests/JobCommandCenter.IntegrationTests/`:

**AspireIntegrationTests.cs:**
```csharp
using FluentAssertions;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using JobCommandCenter.Web;
using System.Net;

namespace JobCommandCenter.IntegrationTests;

public class AspireIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AspireIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task WebService_ShouldBeHealthy()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task WebService_ShouldConnectToDatabase()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<JobCommandCenter.Data.AppDbContext>();

        // Act & Assert
        var canConnect = await dbContext.Database.CanConnectAsync();
        canConnect.Should().BeTrue("the database should be accessible");
    }

    [Fact]
    public async Task WebService_ShouldHaveMigrationsApplied()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<JobCommandCenter.Data.AppDbContext>();

        // Act
        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();

        // Assert
        pendingMigrations.Should().BeEmpty("all migrations should be applied");
    }
}
```

### Manual Verification Checklist

- [ ] Aspire dashboard loads successfully
- [ ] PostgreSQL container starts (check Docker Desktop)
- [ ] Web service starts and shows healthy status
- [ ] Harvester service starts and shows healthy status
- [ ] Web UI loads without database errors
- [ ] Jobs table exists in database
- [ ] HistoryLogs table exists in database
- [ ] No error messages in service logs

---

## Notes

### Design Decisions

1. **Auto-Migration Strategy:** Applying migrations on startup is suitable for development. For production, consider:
   - Manual migration deployment
   - CI/CD pipeline migrations
   - Migration bundles

2. **Development-Only Migration:** The `IsDevelopment()` check prevents accidental production migrations

3. **Shared Database:** Both Web and Harvester share the same database connection string from Aspire

### Aspire Dashboard Features

When running the AppHost, the Aspire dashboard provides:
- **Resources View:** Shows all services and containers
- **Console Logs:** Real-time logs for each resource
- **Traces:** Distributed tracing across services
- **Metrics:** OpenTelemetry metrics visualization

### Common Issues and Solutions

| Issue | Cause | Solution |
|-------|-------|----------|
| "Docker not found" | Docker Desktop not running | Start Docker Desktop |
| "Port already in use" | Another service using the port | Stop conflicting service or change port |
| "Connection refused" | PostgreSQL not ready | Wait for container to fully start |
| "Migration failed" | SQL error in migration | Check migration SQL, fix and regenerate |
| "Service unhealthy" | Health check failing | Check service logs for errors |

### Future Considerations

- Add proper health checks for database connectivity
- Implement graceful shutdown for Harvester
- Add configuration for production connection strings
- Consider using Aspire's `WithHttpHealthCheck()` for more robust health monitoring

---

## References

- [.NET Aspire Overview](https://learn.microsoft.com/en-us/dotnet/aspire/get-started/aspire-overview)
- [Aspire PostgreSQL Integration](https://learn.microsoft.com/en-us/dotnet/aspire/database/postgresql-component)
- [Aspire Service Defaults](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/service-defaults)
- [EF Core Runtime Migration](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying)

---

## Changelog

| Date | Author | Change |
|------|--------|--------|
| 2025-01-14 | Documentation Expert | Initial ticket creation |
