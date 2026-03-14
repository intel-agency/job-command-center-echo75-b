# Story: Define Supporting Enums and Value Objects (Epic 1.2: #5)

## Ticket Metadata

| Field | Value |
|-------|-------|
| **Ticket ID** | 1.2.2 |
| **Epic** | 1.2 - Data Modeling |
| **Story Points** | 1 |
| **Priority** | High |
| **Status** | Ready |
| **Assignee** | TBD |
| **Sprint** | Phase 1 |

---

## Objective

Define the `JobStatus` enum for tracking job pipeline states and the `ScoringConfig` class for configuring the scoring engine weights. These supporting types enable type-safe status transitions and customizable scoring behavior.

---

## Scope

**Files Affected:**
- `src/JobCommandCenter/JobCommandCenter.Shared/Models/JobStatus.cs` (create/update)
- `src/JobCommandCenter/JobCommandCenter.Shared/Models/ScoringConfig.cs` (create/update)

**Out of Scope:**
- ScoringEngine implementation (uses ScoringConfig but is separate)
- Database persistence of ScoringConfig (future consideration)

---

## Context

### JobStatus Enum

The job pipeline moves through discrete states as jobs progress from discovery to application to interview. The `JobStatus` enum provides type-safe status values that prevent invalid states and enable clear filtering/querying.

**Pipeline Flow:**
```
Found → Scored → Pending → Applied → Interviewing → (Archived | Rejected)
```

### ScoringConfig Class

The scoring engine calculates job match scores based on weighted criteria. `ScoringConfig` allows customization of these weights without code changes, enabling users to tune scoring to their preferences.

---

## Technical Specifications

### JobStatus Enum Values

| Value | Integer | Description |
|-------|---------|-------------|
| `Found` | 0 | Job has been scraped but not yet reviewed |
| `Scored` | 1 | Job has been scored by the scoring engine |
| `Pending` | 2 | User has approved the job for application |
| `Applied` | 3 | Application has been submitted |
| `Interviewing` | 4 | Interview has been scheduled |
| `Archived` | 5 | Job is no longer relevant (completed or withdrawn) |
| `Rejected` | 6 | User rejected the job |

### ScoringConfig Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `RemotePoints` | `int` | 50 | Points awarded for remote positions |
| `ContractPoints` | `int` | 0 | Points awarded for contract positions |
| `PromotedPenalty` | `int` | 20 | Points deducted for promoted/sponsored jobs |
| `TopApplicantPoints` | `int` | 30 | Points awarded for "Top Applicant" status |
| `KeywordWeights` | `Dictionary<string, int>` | empty | Keyword → points mapping for description matching |

---

## Plan

### Part A: JobStatus Enum

- [ ] **Step 1: Create or update JobStatus.cs**
  - Navigate to `src/JobCommandCenter/JobCommandCenter.Shared/Models/`
  - Create or update `JobStatus.cs`

- [ ] **Step 2: Define the enum namespace**
  ```csharp
  namespace JobCommandCenter.Shared.Models;
  ```

- [ ] **Step 3: Add XML documentation for the enum**
  - Add class-level `<summary>` describing the pipeline status tracking

- [ ] **Step 4: Define enum values with explicit integers**
  - `Found = 0`
  - `Scored = 1`
  - `Pending = 2`
  - `Applied = 3`
  - `Interviewing = 4`
  - `Archived = 5`
  - `Rejected = 6`

- [ ] **Step 5: Add XML documentation for each enum value**
  - Document the meaning and typical transition triggers for each status

### Part B: ScoringConfig Class

- [ ] **Step 6: Create or update ScoringConfig.cs**
  - Navigate to `src/JobCommandCenter/JobCommandCenter.Shared/Models/`
  - Create or update `ScoringConfig.cs`

- [ ] **Step 7: Define the class structure**
  ```csharp
  namespace JobCommandCenter.Shared.Models;

  /// <summary>
  /// Configuration for the scoring engine weights.
  /// </summary>
  public class ScoringConfig
  {
      // Properties
  }
  ```

- [ ] **Step 8: Add point properties with defaults**
  - `RemotePoints` with default 50
  - `ContractPoints` with default 0
  - `PromotedPenalty` with default 20
  - `TopApplicantPoints` with default 30

- [ ] **Step 9: Add KeywordWeights dictionary**
  - `Dictionary<string, int>` with default empty dictionary

- [ ] **Step 10: Add XML documentation for all properties**
  - Document the purpose and typical values for each weight

- [ ] **Step 11: Verify compilation**
  - Run `dotnet build` to ensure no compilation errors

---

## Acceptance Criteria

### JobStatus

| # | Criterion | Verification |
|---|-----------|--------------|
| 1 | `JobStatus` enum exists in `JobCommandCenter.Shared/Models/JobStatus.cs` | File exists |
| 2 | Enum contains `Found` value with explicit value 0 | Code review |
| 3 | Enum contains `Scored` value with explicit value 1 | Code review |
| 4 | Enum contains `Pending` value with explicit value 2 | Code review |
| 5 | Enum contains `Applied` value with explicit value 3 | Code review |
| 6 | Enum contains `Interviewing` value with explicit value 4 | Code review |
| 7 | Enum contains `Archived` value with explicit value 5 | Code review |
| 8 | Enum contains `Rejected` value with explicit value 6 | Code review |
| 9 | All enum values have XML documentation | Build check |

### ScoringConfig

| # | Criterion | Verification |
|---|-----------|--------------|
| 10 | `ScoringConfig` class exists in `JobCommandCenter.Shared/Models/ScoringConfig.cs` | File exists |
| 11 | Class contains `RemotePoints` property (int, default 50) | Code review |
| 12 | Class contains `ContractPoints` property (int, default 0) | Code review |
| 13 | Class contains `PromotedPenalty` property (int, default 20) | Code review |
| 14 | Class contains `TopApplicantPoints` property (int, default 30) | Code review |
| 15 | Class contains `KeywordWeights` property (Dictionary<string, int>) | Code review |
| 16 | All public members have XML documentation | Build check |

### General

| # | Criterion | Verification |
|---|-----------|--------------|
| 17 | `dotnet build` succeeds with no errors | CI/CD |
| 18 | No compiler warnings | CI/CD |

---

## Validation Commands

```bash
# Build the shared project
cd src/JobCommandCenter/JobCommandCenter.Shared
dotnet build

# Build the entire solution
cd src/JobCommandCenter
dotnet build

# Run unit tests (after tests are created)
dotnet test tests/JobCommandCenter.UnitTests --filter "FullyQualifiedName~JobStatus"
dotnet test tests/JobCommandCenter.UnitTests --filter "FullyQualifiedName~ScoringConfig"
```

---

## Dependencies

| Dependency | Type | Status |
|------------|------|--------|
| None | - | - |

This ticket has no dependencies and can be started in parallel with ticket 1.2.1.

---

## Test Strategy

### Unit Tests

Create unit tests in `tests/JobCommandCenter.UnitTests/Models/`:

**JobStatusTests.cs:**
```csharp
using JobCommandCenter.Shared.Models;
using FluentAssertions;
using Xunit;

namespace JobCommandCenter.UnitTests.Models;

public class JobStatusTests
{
    [Theory]
    [InlineData(JobStatus.Found, 0)]
    [InlineData(JobStatus.Scored, 1)]
    [InlineData(JobStatus.Pending, 2)]
    [InlineData(JobStatus.Applied, 3)]
    [InlineData(JobStatus.Interviewing, 4)]
    [InlineData(JobStatus.Archived, 5)]
    [InlineData(JobStatus.Rejected, 6)]
    public void JobStatus_ShouldHaveExpectedIntValues(JobStatus status, int expectedValue)
    {
        // Arrange & Act
        var actualValue = (int)status;

        // Assert
        actualValue.Should().Be(expectedValue);
    }

    [Fact]
    public void JobStatus_ShouldHaveSevenValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<JobStatus>();

        // Assert
        values.Should().HaveCount(7);
    }

    [Fact]
    public void JobStatus_FoundShouldBeDefault()
    {
        // Arrange & Act
        var defaultStatus = default(JobStatus);

        // Assert
        defaultStatus.Should().Be(JobStatus.Found);
    }
}
```

**ScoringConfigTests.cs:**
```csharp
using JobCommandCenter.Shared.Models;
using FluentAssertions;
using Xunit;

namespace JobCommandCenter.UnitTests.Models;

public class ScoringConfigTests
{
    [Fact]
    public void ScoringConfig_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var config = new ScoringConfig();

        // Assert
        config.RemotePoints.Should().Be(50);
        config.ContractPoints.Should().Be(0);
        config.PromotedPenalty.Should().Be(20);
        config.TopApplicantPoints.Should().Be(30);
        config.KeywordWeights.Should().NotBeNull();
        config.KeywordWeights.Should().BeEmpty();
    }

    [Fact]
    public void ScoringConfig_ShouldAllowCustomWeights()
    {
        // Arrange & Act
        var config = new ScoringConfig
        {
            RemotePoints = 100,
            ContractPoints = 25,
            PromotedPenalty = 50,
            TopApplicantPoints = 75,
            KeywordWeights = new Dictionary<string, int>
            {
                { "senior", 10 },
                { "lead", 15 },
                { "architect", 20 }
            }
        };

        // Assert
        config.RemotePoints.Should().Be(100);
        config.ContractPoints.Should().Be(25);
        config.PromotedPenalty.Should().Be(50);
        config.TopApplicantPoints.Should().Be(75);
        config.KeywordWeights.Should().ContainKey("senior");
        config.KeywordWeights["senior"].Should().Be(10);
        config.KeywordWeights["lead"].Should().Be(15);
        config.KeywordWeights["architect"].Should().Be(20);
    }

    [Fact]
    public void ScoringConfig_KeywordWeightsShouldBeMutable()
    {
        // Arrange
        var config = new ScoringConfig();
        
        // Act
        config.KeywordWeights.Add("python", 5);
        config.KeywordWeights.Add("csharp", 10);

        // Assert
        config.KeywordWeights.Should().HaveCount(2);
        config.KeywordWeights["python"].Should().Be(5);
        config.KeywordWeights["csharp"].Should().Be(10);
    }
}
```

---

## Notes

### Design Decisions

1. **Explicit Enum Values:** Using explicit integer values ensures database values remain stable even if enum order changes
2. **Zero-based Default:** `Found = 0` ensures new jobs default to the initial state
3. **Archived vs Rejected:** Separated into distinct states to enable different filtering and reporting
4. **ScoringConfig as POCO:** Simple class with public setters allows easy configuration via DI or appsettings.json

### Future Considerations

- Consider storing `ScoringConfig` in the database for per-user customization
- May add `IsTerminalState()` helper method to `JobStatus` for workflow logic
- Could add `[Display]` attributes for UI-friendly names in future

---

## References

- [Enum Design Guidelines](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/enum)
- [C# Enums Best Practices](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/enum)

---

## Changelog

| Date | Author | Change |
|------|--------|--------|
| 2025-01-14 | Documentation Expert | Initial ticket creation |
