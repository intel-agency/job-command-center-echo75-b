# Story: Generate Initial EF Core Migration (Epic 1.2: #5)

## Ticket Metadata

| Field | Value |
|-------|-------|
| **Ticket ID** | 1.2.4 |
| **Epic** | 1.2 - Data Modeling |
| **Story Points** | 2 |
| **Priority** | High |
| **Status** | Ready |
| **Assignee** | TBD |
| **Sprint** | Phase 1 |

---

## Objective

Generate the initial Entity Framework Core migration that creates the database schema for the Job Command Center application. This migration will create the `Jobs`, `HistoryLogs`, and related tables with proper indexes, constraints, and foreign key relationships in PostgreSQL.

---

## Scope

**Files Affected:**
- `src/JobCommandCenter/JobCommandCenter.Data/Migrations/` (new folder with generated migration files)
- `src/JobCommandCenter/JobCommandCenter.Data/JobCommandCenter.Data.csproj` (verify EF Core tools reference)

**Out of Scope:**
- AppHost integration (covered in ticket 1.2.5)
- Production database deployment (future consideration)

---

## Context

EF Core migrations provide a way to incrementally update the database schema to keep it in sync with the application's data model. The initial migration creates the baseline schema based on the `AppDbContext` configuration.

**Migration Workflow:**
1. `dotnet ef migrations add InitialCreate` - Generates migration files
2. Review generated migration for correctness
3. `dotnet ef database update` - Applies migration to database
4. Verify schema in PostgreSQL

The generated migration will include:
- `Jobs` table with all columns and indexes
- `HistoryLogs` table with foreign key to Jobs
- Unique constraint on `LinkedInJobId`
- Indexes on `Status`, `Score`, `CreatedAt`

---

## Technical Specifications

### Prerequisites

Ensure the following are installed/configured:

| Requirement | Command/Location |
|-------------|------------------|
| EF Core CLI | `dotnet tool install --global dotnet-ef` |
| PostgreSQL | Running locally or via Docker |
| Connection String | Configured in `appsettings.json` or Aspire |

### Migration Output Structure

```
JobCommandCenter.Data/Migrations/
├── [Timestamp]_InitialCreate.cs           # Main migration logic
└── [Timestamp]_InitialCreate.Designer.cs  # Designer file (auto-generated)
```

### Expected Schema

**Jobs Table:**
| Column | Type | Constraints |
|--------|------|-------------|
| `Id` | uuid | PRIMARY KEY |
| `LinkedInJobId` | character varying(100) | UNIQUE, NOT NULL |
| `Title` | character varying(500) | NOT NULL |
| `Company` | character varying(200) | NOT NULL |
| `Location` | character varying(200) | NULL |
| `IsRemote` | boolean | NOT NULL |
| `IsContract` | boolean | NOT NULL |
| `PayRate` | character varying(100) | NULL |
| `IsTopApplicant` | boolean | NOT NULL |
| `IsPromoted` | boolean | NOT NULL |
| `Description` | text | NULL |
| `Url` | character varying(2000) | NOT NULL |
| `Score` | integer | NOT NULL |
| `Status` | integer | NOT NULL |
| `CreatedAt` | timestamp without time zone | NOT NULL |
| `UpdatedAt` | timestamp without time zone | NOT NULL |

**Indexes on Jobs:**
- `IX_Jobs_LinkedInJobId` (UNIQUE)
- `IX_Jobs_Status`
- `IX_Jobs_Score`
- `IX_Jobs_CreatedAt`

**HistoryLogs Table:**
| Column | Type | Constraints |
|--------|------|-------------|
| `Id` | uuid | PRIMARY KEY |
| `JobId` | uuid | FK → Jobs.Id, NOT NULL |
| `Action` | character varying(100) | NOT NULL |
| `Details` | text | NULL |
| `Timestamp` | timestamp without time zone | NOT NULL |

**Indexes on HistoryLogs:**
- `IX_HistoryLogs_JobId`
- `IX_HistoryLogs_Timestamp`

---

## Plan

### Part A: Setup and Prerequisites

- [ ] **Step 1: Install EF Core CLI tool (if not installed)**
  ```bash
  dotnet tool install --global dotnet-ef
  # Or update if already installed
  dotnet tool update --global dotnet-ef
  ```

- [ ] **Step 2: Verify EF Core CLI version**
  ```bash
  dotnet ef --version
  # Should show version compatible with .NET 10
  ```

- [ ] **Step 3: Ensure Microsoft.EntityFrameworkCore.Design is referenced**
  - Check `JobCommandCenter.Data.csproj` for the package reference
  - Add if missing: `<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="*" />`

- [ ] **Step 4: Verify project builds cleanly**
  ```bash
  cd src/JobCommandCenter
  dotnet build
  ```

### Part B: Generate Migration

- [ ] **Step 5: Navigate to Data project**
  ```bash
  cd src/JobCommandCenter/JobCommandCenter.Data
  ```

- [ ] **Step 6: Generate initial migration**
  ```bash
  dotnet ef migrations add InitialCreate --startup-project ../JobCommandCenter.Web --output-dir Migrations
  ```

- [ ] **Step 7: Review generated migration file**
  - Open `[Timestamp]_InitialCreate.cs`
  - Verify `Up` method creates Jobs and HistoryLogs tables
  - Verify indexes are created correctly
  - Verify unique constraint on LinkedInJobId

- [ ] **Step 8: Review migration SQL (optional)**
  ```bash
  dotnet ef migrations script --startup-project ../JobCommandCenter.Web
  ```
  - Review the SQL for correctness
  - Save to file for documentation if desired

### Part C: Apply and Verify Migration

- [ ] **Step 9: Start PostgreSQL (if not running)**
  - Via Docker: `docker run -d -p 5432:5432 -e POSTGRES_PASSWORD=postgres postgres:14`
  - Or use Aspire-managed instance

- [ ] **Step 10: Configure connection string for migration**
  - Ensure `appsettings.json` in Web project has a valid PostgreSQL connection string
  - Or use environment variable: `ConnectionStrings__jobdb`

- [ ] **Step 11: Apply migration to database**
  ```bash
  dotnet ef database update --startup-project ../JobCommandCenter.Web
  ```

- [ ] **Step 12: Verify schema in PostgreSQL**
  ```bash
  # Using psql
  psql -h localhost -U postgres -d jobcommandcenter -c "\dt"
  psql -h localhost -U postgres -d jobcommandcenter -c "\d \"Jobs\""
  psql -h localhost -U postgres -d jobcommandcenter -c "\d \"HistoryLogs\""
  ```

- [ ] **Step 13: Verify indexes**
  ```sql
  SELECT indexname, indexdef FROM pg_indexes WHERE tablename = 'Jobs';
  ```

### Part D: Commit Migration Files

- [ ] **Step 14: Add migration files to git**
  ```bash
  git add src/JobCommandCenter/JobCommandCenter.Data/Migrations/
  ```

- [ ] **Step 15: Commit with descriptive message**
  ```bash
  git commit -m "feat(data): add initial EF Core migration for Jobs and HistoryLogs"
  ```

---

## Acceptance Criteria

| # | Criterion | Verification |
|---|-----------|--------------|
| 1 | EF Core CLI tool is installed and accessible | `dotnet ef --version` succeeds |
| 2 | Migration files are generated in `Migrations/` folder | Files exist |
| 3 | Migration file contains `Up` method with `CreateTable` for Jobs | Code review |
| 4 | Migration file contains `Up` method with `CreateTable` for HistoryLogs | Code review |
| 5 | Unique index on `LinkedInJobId` is created | SQL verification |
| 6 | Index on `Status` is created | SQL verification |
| 7 | Index on `Score` is created | SQL verification |
| 8 | Index on `CreatedAt` is created | SQL verification |
| 9 | Foreign key from HistoryLogs to Jobs is configured | SQL verification |
| 10 | `dotnet ef database update` succeeds without errors | Command execution |
| 11 | Jobs table exists in PostgreSQL with all columns | `\d "Jobs"` in psql |
| 12 | HistoryLogs table exists in PostgreSQL with all columns | `\d "HistoryLogs"` in psql |
| 13 | Migration `Down` method drops tables correctly | Code review |
| 14 | `dotnet build` succeeds with no errors | CI/CD |

---

## Validation Commands

```bash
# Verify EF Core tools
dotnet ef --version

# Build the solution
cd src/JobCommandCenter
dotnet build

# Generate migration (if needed to regenerate)
cd JobCommandCenter.Data
dotnet ef migrations add InitialCreate --startup-project ../JobCommandCenter.Web --output-dir Migrations

# Generate SQL script for review
dotnet ef migrations script --startup-project ../JobCommandCenter.Web --output migration.sql

# Apply migration
dotnet ef database update --startup-project ../JobCommandCenter.Web

# Check migration status
dotnet ef migrations list --startup-project ../JobCommandCenter.Web

# Rollback (if needed for testing)
dotnet ef database update 0 --startup-project ../JobCommandCenter.Web

# Remove last migration (if needed before applying)
dotnet ef migrations remove --startup-project ../JobCommandCenter.Web
```

---

## Dependencies

| Dependency | Type | Status |
|------------|------|--------|
| Ticket 1.2.3 | AppDbContext implementation | Required |
| PostgreSQL | Infrastructure | Required |
| EF Core CLI | Tooling | Required |

This ticket depends on the AppDbContext being fully implemented in ticket 1.2.3.

---

## Test Strategy

### Integration Tests

Create integration tests in `tests/JobCommandCenter.IntegrationTests/`:

**MigrationTests.cs:**
```csharp
using JobCommandCenter.Data;
using JobCommandCenter.Data.Entities;
using JobCommandCenter.Shared.Models;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Xunit;
using Testcontainers.PostgreSql;

namespace JobCommandCenter.IntegrationTests;

public class MigrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithDatabase("testdb")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private AppDbContext _context = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;
        
        _context = new AppDbContext(options);
        await _context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _container.DisposeAsync();
    }

    [Fact]
    public async Task Migration_ShouldCreateJobsTable()
    {
        // Arrange & Act - Migration already applied in InitializeAsync
        
        // Assert
        var canConnect = await _context.Database.CanConnectAsync();
        canConnect.Should().BeTrue();
        
        var jobs = await _context.Jobs.ToListAsync();
        jobs.Should().BeEmpty(); // Table exists but is empty
    }

    [Fact]
    public async Task Migration_ShouldCreateHistoryLogsTable()
    {
        // Arrange - Create a job first
        var job = new JobEntity
        {
            LinkedInJobId = "test-job",
            Title = "Test",
            Company = "Test Co",
            Url = "https://test.com"
        };
        _context.Jobs.Add(job);
        await _context.SaveChangesAsync();

        // Act
        var historyLog = new HistoryLogEntity
        {
            JobId = job.Id,
            Action = "Created",
            Timestamp = DateTime.UtcNow
        };
        _context.HistoryLogs.Add(historyLog);
        await _context.SaveChangesAsync();

        // Assert
        var retrieved = await _context.HistoryLogs.FirstOrDefaultAsync(h => h.JobId == job.Id);
        retrieved.Should().NotBeNull();
    }

    [Fact]
    public async Task Migration_ShouldEnforceUniqueLinkedInJobId()
    {
        // Arrange
        var job1 = new JobEntity
        {
            LinkedInJobId = "unique-test-id",
            Title = "Job 1",
            Company = "Company 1",
            Url = "https://test.com/1"
        };
        _context.Jobs.Add(job1);
        await _context.SaveChangesAsync();

        var job2 = new JobEntity
        {
            LinkedInJobId = "unique-test-id", // Duplicate!
            Title = "Job 2",
            Company = "Company 2",
            Url = "https://test.com/2"
        };
        _context.Jobs.Add(job2);

        // Act & Assert
        var act = async () => await _context.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>()
            .Where(e => e.InnerException?.Message.Contains("duplicate key") == true 
                        || e.InnerException?.Message.Contains("unique") == true);
    }

    [Fact]
    public async Task Migration_ShouldCreateIndexes()
    {
        // Arrange
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();
        
        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT indexname 
            FROM pg_indexes 
            WHERE tablename = 'Jobs'";
        
        // Act
        var indexes = new List<string>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            indexes.Add(reader.GetString(0));
        }

        // Assert
        indexes.Should().Contain(i => i.Contains("LinkedInJobId"));
        indexes.Should().Contain(i => i.Contains("Status"));
        indexes.Should().Contain(i => i.Contains("Score"));
        indexes.Should().Contain(i => i.Contains("CreatedAt"));
    }
}
```

---

## Notes

### Design Decisions

1. **Migration Naming:** Using `InitialCreate` as the standard name for the first migration
2. **Output Directory:** Explicitly specifying `Migrations` folder for clarity
3. **Startup Project:** Using Web project as startup because it has the DI configuration

### Common Issues and Solutions

| Issue | Cause | Solution |
|-------|-------|----------|
| "Build failed" | Compilation errors | Run `dotnet build` and fix errors first |
| "No executable found" | EF Core tools not installed | Run `dotnet tool install --global dotnet-ef` |
| "Unable to connect" | PostgreSQL not running | Start PostgreSQL container or service |
| "No DbContext found" | Wrong startup project | Specify `--startup-project` correctly |
| Migration generates empty | No DbSet changes | Ensure AppDbContext has DbSets configured |

### Future Considerations

- Consider using migration bundles for deployment
- Add idempotent migration scripts for production
- Set up automatic migration in development environments

---

## References

- [EF Core Migrations Overview](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [EF Core CLI Reference](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)
- [Npgsql EF Core Migrations](https://www.npgsql.org/efcore/migration/index.html)
- [Testcontainers for .NET](https://dotnet.testcontainers.org/)

---

## Changelog

| Date | Author | Change |
|------|--------|--------|
| 2025-01-14 | Documentation Expert | Initial ticket creation |
