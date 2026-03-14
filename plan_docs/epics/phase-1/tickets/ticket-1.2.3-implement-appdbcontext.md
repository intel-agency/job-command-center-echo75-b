# Story: Implement AppDbContext (Epic 1.2: #5)

## Ticket Metadata

| Field | Value |
|-------|-------|
| **Ticket ID** | 1.2.3 |
| **Epic** | 1.2 - Data Modeling |
| **Story Points** | 3 |
| **Priority** | High |
| **Status** | Ready |
| **Assignee** | TBD |
| **Sprint** | Phase 1 |

---

## Objective

Implement the Entity Framework Core `AppDbContext` class that provides database access for the Job Command Center application. This includes configuring DbSets for all entities, setting up Npgsql PostgreSQL provider integration, and defining Fluent API configurations for indexes and constraints.

---

## Scope

**Files Affected:**
- `src/JobCommandCenter/JobCommandCenter.Data/AppDbContext.cs` (create/update)
- `src/JobCommandCenter/JobCommandCenter.Data/Entities/JobEntity.cs` (create/update)
- `src/JobCommandCenter/JobCommandCenter.Data/Entities/HistoryLogEntity.cs` (create)
- `src/JobCommandCenter/JobCommandCenter.Data/JobCommandCenter.Data.csproj` (verify packages)

**Out of Scope:**
- Migration generation (covered in ticket 1.2.4)
- Service registration in Web/Harvester (covered in ticket 1.2.5)

---

## Context

The `AppDbContext` is the central point for database access using Entity Framework Core. It:

1. Defines `DbSet<T>` properties for each entity type
2. Configures entity mappings via Fluent API in `OnModelCreating`
3. Integrates with Npgsql for PostgreSQL-specific features
4. Works with Aspire's dependency injection for connection string management

The Data project follows a separation pattern where:
- **Domain Models** (in Shared) represent business concepts
- **Entity Classes** (in Data/Entities) represent database tables with EF Core attributes
- **AppDbContext** bridges the two with mapping configuration

---

## Technical Specifications

### AppDbContext Structure

```csharp
public class AppDbContext : DbContext
{
    public DbSet<JobEntity> Jobs => Set<JobEntity>();
    public DbSet<HistoryLogEntity> HistoryLogs => Set<HistoryLogEntity>();
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Fluent API configuration
    }
}
```

### Entity Configuration Requirements

#### JobEntity

| Configuration | Value |
|---------------|-------|
| Primary Key | `Id` (Guid) |
| Unique Index | `LinkedInJobId` |
| Index | `Status` |
| Index | `Score` |
| Index | `CreatedAt` |
| Max Length | `LinkedInJobId`: 100, `Title`: 500, `Company`: 200, `Url`: 2000 |

#### HistoryLogEntity (New)

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `Guid` | Primary key |
| `JobId` | `Guid` | Foreign key to Job |
| `Action` | `string` | Action performed (e.g., "Created", "Scored", "Applied") |
| `Details` | `string?` | Additional details |
| `Timestamp` | `DateTime` | When the action occurred |

### Required NuGet Packages

Verify these packages are referenced in `JobCommandCenter.Data.csproj`:
- `Microsoft.EntityFrameworkCore` (transitive)
- `Npgsql.EntityFrameworkCore.PostgreSQL`
- `Aspire.Npgsql.EntityFrameworkCore.PostgreSQL`

---

## Plan

### Part A: Create HistoryLogEntity

- [ ] **Step 1: Create HistoryLogEntity.cs**
  - Navigate to `src/JobCommandCenter/JobCommandCenter.Data/Entities/`
  - Create `HistoryLogEntity.cs`

- [ ] **Step 2: Define the entity class**
  ```csharp
  namespace JobCommandCenter.Data.Entities;

  /// <summary>
  /// Audit trail entry for job state changes.
  /// </summary>
  public class HistoryLogEntity
  {
      public Guid Id { get; set; }
      public Guid JobId { get; set; }
      public string Action { get; set; } = string.Empty;
      public string? Details { get; set; }
      public DateTime Timestamp { get; set; }
      public JobEntity? Job { get; set; }
  }
  ```

- [ ] **Step 3: Add XML documentation**
  - Document all properties with `<summary>` comments

### Part B: Update JobEntity

- [ ] **Step 4: Review and update JobEntity.cs**
  - Ensure all properties match domain model
  - Add data annotations if needed
  - Verify `JobStatus` enum usage

- [ ] **Step 5: Add navigation property for HistoryLogs**
  - Add `public ICollection<HistoryLogEntity> HistoryLogs { get; set; }` to JobEntity

- [ ] **Step 6: Add XML documentation**
  - Ensure all properties are documented

### Part C: Implement AppDbContext

- [ ] **Step 7: Create or update AppDbContext.cs**
  - Navigate to `src/JobCommandCenter/JobCommandCenter.Data/`
  - Create or update `AppDbContext.cs`

- [ ] **Step 8: Define the class structure**
  ```csharp
  using JobCommandCenter.Data.Entities;
  using Microsoft.EntityFrameworkCore;

  namespace JobCommandCenter.Data;

  /// <summary>
  /// Entity Framework Core database context for Job Command Center.
  /// </summary>
  public class AppDbContext : DbContext
  {
      // DbSets and configuration
  }
  ```

- [ ] **Step 9: Add DbSet properties**
  - `public DbSet<JobEntity> Jobs => Set<JobEntity>();`
  - `public DbSet<HistoryLogEntity> HistoryLogs => Set<HistoryLogEntity>();`

- [ ] **Step 10: Add constructor**
  - `public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }`

- [ ] **Step 11: Override OnModelCreating for JobEntity**
  - Configure primary key
  - Configure unique index on `LinkedInJobId`
  - Configure indexes on `Status`, `Score`, `CreatedAt`
  - Configure max lengths for string properties
  - Configure relationship to HistoryLogs

- [ ] **Step 12: Override OnModelCreating for HistoryLogEntity**
  - Configure primary key
  - Configure relationship to JobEntity
  - Configure index on `JobId`
  - Configure index on `Timestamp`

- [ ] **Step 13: Add XML documentation**
  - Document class and all public members

- [ ] **Step 14: Verify NuGet packages**
  - Ensure `Aspire.Npgsql.EntityFrameworkCore.PostgreSQL` is referenced

- [ ] **Step 15: Verify compilation**
  - Run `dotnet build` to ensure no errors

---

## Acceptance Criteria

| # | Criterion | Verification |
|---|-----------|--------------|
| 1 | `AppDbContext` class exists in `JobCommandCenter.Data/AppDbContext.cs` | File exists |
| 2 | `AppDbContext` inherits from `DbContext` | Code review |
| 3 | `DbSet<JobEntity> Jobs` property exists | Code review |
| 4 | `DbSet<HistoryLogEntity> HistoryLogs` property exists | Code review |
| 5 | Constructor accepts `DbContextOptions<AppDbContext>` | Code review |
| 6 | `OnModelCreating` configures JobEntity primary key | Code review |
| 7 | Unique index on `LinkedInJobId` is configured | Code review |
| 8 | Index on `Status` is configured | Code review |
| 9 | Index on `Score` is configured | Code review |
| 10 | Index on `CreatedAt` is configured | Code review |
| 11 | `HistoryLogEntity` class exists with required properties | Code review |
| 12 | Foreign key relationship between HistoryLog and Job is configured | Code review |
| 13 | All public members have XML documentation | Build check |
| 14 | `dotnet build` succeeds with no errors | CI/CD |
| 15 | Npgsql package is correctly referenced | csproj review |

---

## Validation Commands

```bash
# Build the Data project
cd src/JobCommandCenter/JobCommandCenter.Data
dotnet build

# Build the entire solution
cd src/JobCommandCenter
dotnet build

# Verify EF Core can create a model (dry run)
cd src/JobCommandCenter/JobCommandCenter.Data
dotnet ef dbcontext info --startup-project ../JobCommandCenter.Web
```

---

## Dependencies

| Dependency | Type | Status |
|------------|------|--------|
| Ticket 1.2.1 | Job domain model | Required |
| Ticket 1.2.2 | JobStatus enum | Required |

This ticket depends on the domain models being defined in tickets 1.2.1 and 1.2.2.

---

## Test Strategy

### Unit Tests

Create unit tests in `tests/JobCommandCenter.UnitTests/Data/`:

**AppDbContextTests.cs:**
```csharp
using JobCommandCenter.Data;
using JobCommandCenter.Data.Entities;
using JobCommandCenter.Shared.Models;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Xunit;

namespace JobCommandCenter.UnitTests.Data;

public class AppDbContextTests
{
    private AppDbContext CreateInMemoryContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task AppDbContext_ShouldAddAndRetrieveJob()
    {
        // Arrange
        await using var context = CreateInMemoryContext("TestDb_AddJob");
        var job = new JobEntity
        {
            LinkedInJobId = "test-123",
            Title = "Software Engineer",
            Company = "Test Corp",
            Url = "https://linkedin.com/jobs/test-123"
        };

        // Act
        context.Jobs.Add(job);
        await context.SaveChangesAsync();

        // Assert
        var retrieved = await context.Jobs.FirstOrDefaultAsync(j => j.LinkedInJobId == "test-123");
        retrieved.Should().NotBeNull();
        retrieved!.Title.Should().Be("Software Engineer");
    }

    [Fact]
    public async Task AppDbContext_ShouldEnforceUniqueLinkedInJobId()
    {
        // Arrange
        await using var context = CreateInMemoryContext("TestDb_UniqueJobId");
        var job1 = new JobEntity
        {
            LinkedInJobId = "duplicate-id",
            Title = "Job 1",
            Company = "Company 1",
            Url = "https://linkedin.com/jobs/1"
        };
        var job2 = new JobEntity
        {
            LinkedInJobId = "duplicate-id",
            Title = "Job 2",
            Company = "Company 2",
            Url = "https://linkedin.com/jobs/2"
        };

        // Act
        context.Jobs.Add(job1);
        await context.SaveChangesAsync();
        context.Jobs.Add(job2);

        // Assert
        // Note: InMemory provider doesn't enforce unique constraints
        // This test verifies the configuration exists; actual enforcement tested in integration tests
        var model = context.Model;
        var jobEntity = model.FindEntityType(typeof(JobEntity));
        var linkedInJobIdIndex = jobEntity?.GetIndexes()
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == "LinkedInJobId"));
        
        linkedInJobIdIndex.Should().NotBeNull();
        linkedInJobIdIndex!.IsUnique.Should().BeTrue();
    }

    [Fact]
    public async Task AppDbContext_ShouldAddHistoryLogWithJobRelationship()
    {
        // Arrange
        await using var context = CreateInMemoryContext("TestDb_HistoryLog");
        var job = new JobEntity
        {
            LinkedInJobId = "job-with-history",
            Title = "Test Job",
            Company = "Test Company",
            Url = "https://linkedin.com/jobs/test"
        };

        // Act
        context.Jobs.Add(job);
        await context.SaveChangesAsync();

        var historyLog = new HistoryLogEntity
        {
            JobId = job.Id,
            Action = "Created",
            Timestamp = DateTime.UtcNow
        };
        context.HistoryLogs.Add(historyLog);
        await context.SaveChangesAsync();

        // Assert
        var retrievedLog = await context.HistoryLogs
            .Include(h => h.Job)
            .FirstOrDefaultAsync(h => h.JobId == job.Id);
        
        retrievedLog.Should().NotBeNull();
        retrievedLog!.Job.Should().NotBeNull();
        retrievedLog.Job!.Title.Should().Be("Test Job");
    }

    [Fact]
    public void AppDbContext_ShouldHaveJobsDbSet()
    {
        // Arrange
        using var context = CreateInMemoryContext("TestDb_DbSets");

        // Act & Assert
        context.Jobs.Should().NotBeNull();
        context.HistoryLogs.Should().NotBeNull();
    }
}
```

---

## Notes

### Design Decisions

1. **Separate Entity Classes:** Entity classes are separate from domain models to allow different persistence concerns (navigation properties, EF attributes) without polluting the domain layer

2. **Fluent API over Data Annotations:** Using Fluent API in `OnModelCreating` keeps entity classes clean and allows more complex configurations

3. **Navigation Properties:** Bidirectional navigation (Job → HistoryLogs, HistoryLog → Job) enables efficient querying with Include()

4. **InMemory Provider Limitation:** Note that EF Core's InMemory provider doesn't enforce all database constraints (like unique indexes). Integration tests with a real PostgreSQL instance are needed to verify constraint enforcement.

### Future Considerations

- Add `DbSet<ScoringConfigEntity>` for persistent scoring configuration
- Consider adding `AuditDbContext` pattern for automatic audit logging
- May add soft delete support with `IsDeleted` flag and query filters

---

## References

- [EF Core DbContext Configuration](https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/)
- [EF Core Fluent API](https://learn.microsoft.com/en-us/ef/core/modeling/)
- [Npgsql EF Core Provider](https://www.npgsql.org/efcore/)
- [.NET Aspire PostgreSQL Integration](https://learn.microsoft.com/en-us/dotnet/aspire/database/postgresql-component)

---

## Changelog

| Date | Author | Change |
|------|--------|--------|
| 2025-01-14 | Documentation Expert | Initial ticket creation |
