# Story: Define Job Domain Model (Epic 1.2: #5)

## Ticket Metadata

| Field | Value |
|-------|-------|
| **Ticket ID** | 1.2.1 |
| **Epic** | 1.2 - Data Modeling |
| **Story Points** | 2 |
| **Priority** | High |
| **Status** | Ready |
| **Assignee** | TBD |
| **Sprint** | Phase 1 |

---

## Objective

Define the `Job` domain model in the shared library with all required fields for capturing LinkedIn job listing data. This model serves as the canonical representation of a job across all services (Web, Harvester, future APIs).

---

## Scope

**Files Affected:**
- `src/JobCommandCenter/JobCommandCenter.Shared/Models/Job.cs` (create/update)

**Out of Scope:**
- Database entity configuration (covered in ticket 1.2.3)
- Migrations (covered in ticket 1.2.4)
- API contracts or DTOs

---

## Context

The `Job` model is the core domain entity for the entire application. It must capture all relevant data from LinkedIn job listings to support:
- Job scraping and storage
- Scoring and filtering
- Application tracking
- Historical analysis

The model lives in `JobCommandCenter.Shared` to enable reuse across the Harvester (which creates jobs) and Web (which displays/manages jobs).

---

## Technical Specifications

### Required Properties

| Property | Type | Nullable | Required | Description |
|----------|------|----------|----------|-------------|
| `Id` | `Guid` | No | Yes | Primary key (auto-generated) |
| `LinkedInJobId` | `string` | No | Yes | LinkedIn's unique job identifier |
| `Title` | `string` | No | Yes | Job title |
| `Company` | `string` | No | Yes | Company name |
| `Location` | `string?` | Yes | No | Job location (city, state, etc.) |
| `IsRemote` | `bool` | No | No | Whether the job is remote |
| `IsContract` | `bool` | No | No | Whether the job is a contract position |
| `PayRate` | `string?` | Yes | No | Pay rate/salary information |
| `IsTopApplicant` | `bool` | No | No | Whether user is a "Top Applicant" |
| `IsPromoted` | `bool` | No | No | Whether the job is promoted/sponsored |
| `Description` | `string?` | Yes | No | Full job description text |
| `Url` | `string` | No | Yes | Direct URL to the job posting |
| `Score` | `int` | No | No | Calculated score from scoring engine |
| `Status` | `JobStatus` | No | No | Current pipeline status (default: Found) |
| `CreatedAt` | `DateTime` | No | No | Timestamp when job was discovered |
| `UpdatedAt` | `DateTime` | No | No | Timestamp when job was last modified |

### Design Decisions

1. **Required Properties:** Use C# 12 `required` modifier for mandatory fields to enforce initialization at compile time
2. **Nullable Reference Types:** Enable `<Nullable>enable</Nullable>` and mark optional fields as nullable
3. **Default Values:** Provide sensible defaults for boolean flags and timestamps
4. **Immutable by Convention:** Setters are public for EF Core, but should be treated as controlled mutation

---

## Plan

- [ ] **Step 1: Create or update the Job.cs file**
  - Navigate to `src/JobCommandCenter/JobCommandCenter.Shared/Models/`
  - Create or update `Job.cs` with the namespace `JobCommandCenter.Shared.Models`

- [ ] **Step 2: Define the Job class structure**
  ```csharp
  namespace JobCommandCenter.Shared.Models;

  /// <summary>
  /// Represents a job listing from LinkedIn.
  /// </summary>
  public class Job
  {
      // Properties with XML documentation
  }
  ```

- [ ] **Step 3: Add primary key property**
  - Add `Id` property as `Guid` with `Guid.NewGuid()` default

- [ ] **Step 4: Add LinkedIn-specific properties**
  - Add `LinkedInJobId` (required string)
  - Add `Title` (required string)
  - Add `Company` (required string)
  - Add `Url` (required string)

- [ ] **Step 5: Add job details properties**
  - Add `Location` (nullable string)
  - Add `PayRate` (nullable string)
  - Add `Description` (nullable string)

- [ ] **Step 6: Add boolean flag properties**
  - Add `IsRemote` (bool, default false)
  - Add `IsContract` (bool, default false)
  - Add `IsTopApplicant` (bool, default false)
  - Add `IsPromoted` (bool, default false)

- [ ] **Step 7: Add scoring and status properties**
  - Add `Score` (int, default 0)
  - Add `Status` (JobStatus, default JobStatus.Found)

- [ ] **Step 8: Add timestamp properties**
  - Add `CreatedAt` (DateTime, default DateTime.UtcNow)
  - Add `UpdatedAt` (DateTime, default DateTime.UtcNow)

- [ ] **Step 9: Add XML documentation**
  - Add `<summary>` documentation for the class
  - Add `<summary>` documentation for each property

- [ ] **Step 10: Verify compilation**
  - Run `dotnet build` to ensure no compilation errors

---

## Acceptance Criteria

| # | Criterion | Verification |
|---|-----------|--------------|
| 1 | `Job` class exists in `JobCommandCenter.Shared/Models/Job.cs` | File exists and compiles |
| 2 | Class contains `Id` property of type `Guid` | Code review |
| 3 | Class contains `LinkedInJobId` property (required string) | Code review |
| 4 | Class contains `Title` property (required string) | Code review |
| 5 | Class contains `Company` property (required string) | Code review |
| 6 | Class contains `Location` property (nullable string) | Code review |
| 7 | Class contains `PayRate` property (nullable string) | Code review |
| 8 | Class contains `IsRemote` boolean property | Code review |
| 9 | Class contains `IsContract` boolean property | Code review |
| 10 | Class contains `IsTopApplicant` boolean property | Code review |
| 11 | Class contains `IsPromoted` boolean property | Code review |
| 12 | Class contains `Description` property (nullable string) | Code review |
| 13 | Class contains `Url` property (required string) | Code review |
| 14 | Class contains `Score` integer property | Code review |
| 15 | Class contains `Status` property of type `JobStatus` | Code review |
| 16 | Class contains `CreatedAt` DateTime property | Code review |
| 17 | Class contains `UpdatedAt` DateTime property | Code review |
| 18 | All public members have XML documentation | Build generates XML docs |
| 19 | Required properties use `required` modifier | Code review |
| 20 | `dotnet build` succeeds with no errors | CI/CD |

---

## Validation Commands

```bash
# Build the shared project
cd src/JobCommandCenter/JobCommandCenter.Shared
dotnet build

# Build the entire solution
cd src/JobCommandCenter
dotnet build

# Verify nullable warnings are enabled
dotnet build /warnaserror
```

---

## Dependencies

| Dependency | Type | Status |
|------------|------|--------|
| None | - | - |

This ticket has no dependencies and can be started immediately.

---

## Test Strategy

### Unit Tests

Create unit tests in `tests/JobCommandCenter.UnitTests/Models/JobTests.cs`:

```csharp
using JobCommandCenter.Shared.Models;
using FluentAssertions;
using Xunit;

namespace JobCommandCenter.UnitTests.Models;

public class JobTests
{
    [Fact]
    public void Job_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var job = new Job
        {
            LinkedInJobId = "test-123",
            Title = "Software Engineer",
            Company = "Test Corp",
            Url = "https://linkedin.com/jobs/test-123"
        };

        // Assert
        job.Id.Should().NotBe(Guid.Empty);
        job.Status.Should().Be(JobStatus.Found);
        job.Score.Should().Be(0);
        job.IsRemote.Should().BeFalse();
        job.IsContract.Should().BeFalse();
        job.IsTopApplicant.Should().BeFalse();
        job.IsPromoted.Should().BeFalse();
        job.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        job.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Job_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var expectedId = Guid.NewGuid();
        var expectedCreatedAt = DateTime.UtcNow.AddDays(-1);

        // Act
        var job = new Job
        {
            Id = expectedId,
            LinkedInJobId = "linkedin-456",
            Title = "Senior Developer",
            Company = "Acme Inc",
            Location = "San Francisco, CA",
            PayRate = "$150k - $200k",
            IsRemote = true,
            IsContract = false,
            IsTopApplicant = true,
            IsPromoted = false,
            Description = "We are looking for...",
            Url = "https://linkedin.com/jobs/456",
            Score = 85,
            Status = JobStatus.Scored,
            CreatedAt = expectedCreatedAt,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        job.Id.Should().Be(expectedId);
        job.LinkedInJobId.Should().Be("linkedin-456");
        job.Title.Should().Be("Senior Developer");
        job.Company.Should().Be("Acme Inc");
        job.Location.Should().Be("San Francisco, CA");
        job.PayRate.Should().Be("$150k - $200k");
        job.IsRemote.Should().BeTrue();
        job.IsContract.Should().BeFalse();
        job.IsTopApplicant.Should().BeTrue();
        job.IsPromoted.Should().BeFalse();
        job.Description.Should().Be("We are looking for...");
        job.Url.Should().Be("https://linkedin.com/jobs/456");
        job.Score.Should().Be(85);
        job.Status.Should().Be(JobStatus.Scored);
        job.CreatedAt.Should().Be(expectedCreatedAt);
    }
}
```

---

## Notes

- The `required` modifier in C# 11+ ensures that mandatory properties must be set during object initialization
- Nullable reference types help catch potential null reference exceptions at compile time
- The `Score` property will be populated by the `ScoringEngine` service (separate concern)
- Consider adding domain methods like `MarkAsApplied()` in future iterations to encapsulate status transitions

---

## References

- [C# 12 Required Properties](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/required)
- [Nullable Reference Types](https://learn.microsoft.com/en-us/dotnet/csharp/nullable-references)
- [Microsoft C# Naming Conventions](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/naming-guidelines)

---

## Changelog

| Date | Author | Change |
|------|--------|--------|
| 2025-01-14 | Documentation Expert | Initial ticket creation |
