# Job Command Center – Phase 1 – 1.2 Epic: Data Modeling

## Overview

This epic establishes the foundational data layer for the Job Command Center application, creating the domain models, EF Core database context, and migrations required to persist job listings scraped from LinkedIn. The data modeling layer bridges the gap between the domain model (shared across services) and the persistence layer (PostgreSQL), enabling both the Web dashboard and Harvester worker to store and retrieve job data through a unified data access layer.

**Problem Solved:** Without a structured data model and persistence layer, scraped job listings cannot be stored, queried, or tracked across application restarts. This epic establishes the canonical data schema that all services will depend on.

**Component:** JobCommandCenter.Shared (domain models) and JobCommandCenter.Data (EF Core DbContext + migrations)

**Desired Outcomes:**
- A well-defined `Job` domain model with all required fields for LinkedIn job tracking
- Supporting enums (`JobStatus`) and entities (`ScoringConfig`) for pipeline state management
- A configured `AppDbContext` with proper EF Core mappings and Npgsql integration
- Initial database migration that creates the Jobs, HistoryLog, and ScoringConfig tables
- Full integration with .NET Aspire for dependency injection and database provisioning

---

## Project

**Job Command Center** – A local-first LinkedIn job automation platform that attaches to an existing Chrome session via Chrome DevTools Protocol (CDP). Built on .NET 10 with Aspire orchestration, Blazor Server UI, and PostgreSQL persistence.

---

## Component

- **JobCommandCenter.Shared/Models/** – Domain models (`Job`, `JobStatus`, `ScoringConfig`) shared across all services
- **JobCommandCenter.Data/** – EF Core DbContext (`AppDbContext`), entity classes (`JobEntity`), and database migrations

---

## Goals

- [ ] Define a comprehensive `Job` domain model capturing all LinkedIn job attributes
- [ ] Create `JobStatus` enum covering all pipeline stages (Found → Archived)
- [ ] Implement `ScoringConfig` entity for configurable scoring weights
- [ ] Build `AppDbContext` with proper `DbSet<T>` definitions and Fluent API configurations
- [ ] Generate and validate initial EF Core migration for PostgreSQL
- [ ] Wire `AppDbContext` into Aspire AppHost for automatic database provisioning
- [ ] Ensure unique constraints prevent duplicate job entries
- [ ] Add `HistoryLog` table for audit trail functionality
- [ ] Achieve clean `dotnet build` with XML documentation for all public APIs
- [ ] Establish unit test coverage for domain models

---

## Brief Technology Stack

| Layer | Technology |
|-------|------------|
| Language | C# 12 |
| Framework | .NET 10 |
| Database | PostgreSQL |
| ORM | Entity Framework Core with Npgsql provider |
| Orchestration | .NET Aspire |
| Testing | xUnit, FluentAssertions |
| Migrations | EF Core CLI (`dotnet ef`) |

---

## Epic Stories

- Define Job domain model with all LinkedIn job attributes (title, company, location, pay rate, match level, status, external apply flag, URLs, timestamps)
- Define supporting enums (JobStatus) and value objects (ScoringConfig) for pipeline state management
- Implement AppDbContext with DbSets for Jobs, HistoryLog, and ScoringConfig using Npgsql Fluent API
- Generate initial EF Core migration that creates the database schema with proper indexes and constraints
- Wire AppDbContext into Aspire AppHost and ServiceDefaults for automatic database connectivity

---

## Component Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                     JobCommandCenter.Shared                         │
├─────────────────────────────────────────────────────────────────────┤
│  Models/                                                            │
│  ├── Job.cs              (Domain model for job listings)            │
│  ├── JobStatus.cs        (Enum: Found, Scored, Pending, Applied...) │
│  └── ScoringConfig.cs    (Scoring engine weight configuration)      │
└─────────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────────┐
│                      JobCommandCenter.Data                          │
├─────────────────────────────────────────────────────────────────────┤
│  AppDbContext.cs          (EF Core DbContext with DbSets)           │
│  Entities/                                                          │
│  ├── JobEntity.cs         (EF Core entity with indexes)             │
│  └── HistoryLogEntity.cs  (Audit trail entity)                      │
│  Migrations/                                                        │
│  └── [Timestamp]_InitialCreate.cs  (Initial schema migration)       │
└─────────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────────┐
│                    JobCommandCenter.AppHost                         │
├─────────────────────────────────────────────────────────────────────┤
│  AppHost.cs               (Aspire orchestrator)                     │
│  └── AddPostgres("postgres") → AddDatabase("jobdb")                │
│      ├── Web service → .WithReference(db)                           │
│      └── Harvester service → .WithReference(db)                     │
└─────────────────────────────────────────────────────────────────────┘
```

---

## Project Structure Area

```
src/JobCommandCenter/
├── JobCommandCenter.Shared/
│   ├── Models/
│   │   ├── Job.cs                 ← Define/Update
│   │   ├── JobStatus.cs           ← Define/Update
│   │   └── ScoringConfig.cs       ← Define/Update
│   └── Services/
│       └── ScoringEngine.cs       (Uses ScoringConfig)
├── JobCommandCenter.Data/
│   ├── AppDbContext.cs            ← Define/Update
│   ├── Entities/
│   │   ├── JobEntity.cs           ← Define/Update
│   │   └── HistoryLogEntity.cs    ← Create (new)
│   └── Migrations/                ← Generate
│       └── [Timestamp]_InitialCreate.cs
├── JobCommandCenter.AppHost/
│   └── AppHost.cs                 ← Verify/Update
├── JobCommandCenter.ServiceDefaults/
│   └── Extensions.cs              (Existing, no changes)
├── JobCommandCenter.Web/
│   └── Program.cs                 ← Verify DB registration
├── JobCommandCenter.Harvester/
│   └── Program.cs                 ← Verify DB registration
└── tests/
    └── JobCommandCenter.UnitTests/
        └── Models/                ← Create unit tests
```

---

## Implementation Plan

- [ ] **1.2.1 Define Job domain model in JobCommandCenter.Shared/Models/Job.cs**
  - Add all required properties: Title, Company, Location, PayRate, MatchLevel (Score), Status, ExternalApply, LinkedInJobId, Url, Description, CreatedAt, UpdatedAt
  - Use `required` modifier for mandatory fields
  - Enable nullable reference types
  - Add XML documentation for all public members

- [ ] **1.2.2 Define JobStatus enum and ScoringConfig entity**
  - Create `JobStatus` enum with values: Found (0), Scored (1), Pending (2), Applied (3), Interviewing (4), Archived (5), Rejected (6)
  - Create `ScoringConfig` class with RemotePoints, ContractPoints, PromotedPenalty, TopApplicantPoints, and KeywordWeights dictionary
  - Add XML documentation for enum values and class properties

- [ ] **1.2.3 Implement AppDbContext with DbSets and Fluent API**
  - Create `AppDbContext` inheriting from `DbContext`
  - Add `DbSet<JobEntity>` for Jobs table
  - Add `DbSet<HistoryLogEntity>` for audit trail
  - Configure Npgsql with `UseNpgsql()` in `OnConfiguring` or via DI
  - Use Fluent API in `OnModelCreating` for:
    - Primary key configuration
    - Unique index on `LinkedInJobId`
    - Indexes on `Status`, `Score`, `CreatedAt` for query performance
  - Add XML documentation for the context class

- [ ] **1.2.4 Generate initial EF Core migration**
  - Install `Microsoft.EntityFrameworkCore.Design` package if needed
  - Run `dotnet ef migrations add InitialCreate --project JobCommandCenter.Data --startup-project JobCommandCenter.Web`
  - Verify generated migration creates Jobs, HistoryLog, and ScoringConfig tables
  - Apply migration locally: `dotnet ef database update`
  - Verify schema in PostgreSQL

- [ ] **1.2.5 Wire AppDbContext into Aspire AppHost and ServiceDefaults**
  - Verify AppHost configures PostgreSQL: `builder.AddPostgres("postgres").AddDatabase("jobdb")`
  - Verify Web and Harvester projects reference the database
  - Ensure connection string is injected via Aspire's `AddNpgsqlDbContext` extension
  - Add automatic migration on startup: `dbContext.Database.MigrateAsync()`
  - Test by running `dotnet run --project JobCommandCenter.AppHost`

---

## Mandatory Requirements

### Testing
- **Unit Tests:** Create unit tests for `Job` domain model validating property initialization and default values
- **Unit Tests:** Create unit tests for `JobStatus` enum verifying all expected values exist
- **Integration Tests:** Verify database connectivity by running integration test that performs CRUD operations
- **Test Coverage:** Aim for minimum 80% coverage on domain models and DbContext

### Documentation
- **XML Docs:** All public classes, properties, and methods must have XML documentation (`/// <summary>` format)
- **README:** Update project README with data model diagram or description
- **Inline Comments:** Add comments for complex Fluent API configurations

### Build
- **Clean Build:** `dotnet build` must pass with zero warnings (warnings as errors in release)
- **Code Style:** Follow Microsoft C# naming conventions
- **Nullable:** All projects must have `<Nullable>enable</Nullable>`

### Infrastructure
- **Aspire PostgreSQL:** Use Aspire-managed PostgreSQL container via `AddPostgres()`
- **Connection String:** Injected automatically by Aspire, no hardcoded connection strings
- **Migrations:** Stored in `JobCommandCenter.Data/Migrations/` folder

---

## Acceptance Criteria

| # | Criterion | Verification |
|---|-----------|--------------|
| 1 | `Job` entity exists with all required fields: Title, Company, Location, PayRate, Score (MatchLevel), Status, IsRemote, IsContract, IsTopApplicant, IsPromoted, LinkedInJobId, Url, Description, CreatedAt, UpdatedAt | Code review + unit tests |
| 2 | `AppDbContext` implemented with `DbSet<JobEntity>` and Npgsql configuration | Code review |
| 3 | Unique constraint on `LinkedInJobId` prevents duplicate job entries | Integration test attempting duplicate insert |
| 4 | `JobStatus` enum covers all pipeline stages: Found, Scored, Pending, Applied, Interviewing, Archived, Rejected | Code review + unit test |
| 5 | `HistoryLog` table exists for audit trail | Migration file review + database inspection |
| 6 | `ScoringConfig` entity exists with weight properties | Code review |
| 7 | Initial migration applies cleanly to PostgreSQL | `dotnet ef database update` succeeds |
| 8 | Aspire AppHost launches PostgreSQL container and services connect successfully | `dotnet run --project JobCommandCenter.AppHost` |
| 9 | All public APIs have XML documentation | Build generates XML documentation file |
| 10 | `dotnet build` passes with zero warnings | CI/CD pipeline |

---

## Risk Mitigation Strategies

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| EF Core migration fails due to PostgreSQL version incompatibility | High | Low | Use Npgsql provider version compatible with PostgreSQL 14+; test locally before committing |
| Missing unique constraint causes duplicate jobs | High | Medium | Add unique index on `LinkedInJobId` in Fluent API; write integration test for duplicate detection |
| Nullable reference types cause runtime null exceptions | Medium | Medium | Enable `<Nullable>enable</Nullable>` and use `required` modifier on mandatory fields; code review |
| Aspire PostgreSQL container fails to start | High | Low | Verify Docker is running; use Aspire's built-in health checks |
| Migration creates incorrect schema | High | Medium | Review generated migration SQL before applying; use `dotnet ef migrations script` to preview |
| Performance issues with large job volumes | Medium | Low | Add indexes on frequently queried columns (Status, Score, CreatedAt) |
| Breaking changes to domain model affect multiple services | High | Low | Keep domain model in shared project; use semantic versioning for breaking changes |

---

## Timeline Estimate

| Story | Estimated Time |
|-------|----------------|
| 1.2.1 Define Job domain model | 2 hours |
| 1.2.2 Define enums and value objects | 1 hour |
| 1.2.3 Implement AppDbContext | 3 hours |
| 1.2.4 Generate EF Core migration | 2 hours |
| 1.2.5 Wire into Aspire AppHost | 2 hours |
| **Total** | **10 hours** |

---

## Success Metrics

| Metric | Target | Measurement |
|--------|--------|-------------|
| Build success rate | 100% | CI/CD pipeline status |
| Test pass rate | 100% | `dotnet test` output |
| Code coverage (domain models) | ≥80% | Coverage report |
| Migration apply time | <5 seconds | Local execution time |
| Aspire startup time | <30 seconds | Time to dashboard availability |
| Database connection success | 100% | Health check endpoint |
| XML documentation coverage | 100% of public APIs | Build warnings check |

---

## Related Documents

- [Architecture Document](../../architecture.md)
- [Tech Stack](../../tech-stack.md)
- [Development Plan](../../Development%20Plan%20-%20Job%20Command%20Center.md)
- [AGENTS.md](../../../AGENTS.md)

---

## Changelog

| Date | Author | Change |
|------|--------|--------|
| 2025-01-14 | Documentation Expert | Initial epic creation |
