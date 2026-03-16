using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using JobCommandCenter.Shared.Models;
using Xunit;

namespace JobCommandCenter.UnitTests.Shared;

/// <summary>
/// Unit tests for the Job model.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "Domain Models")]
public class JobTests
{
    #region Default Values Tests

    [Fact]
    public void Job_DefaultConstructor_ShouldSetIdToNonEmptyGuid()
    {
        // Arrange & Act
        var job = CreateValidJob();

        // Assert
        job.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Job_DefaultConstructor_ShouldSetStatusToFound()
    {
        // Arrange & Act
        var job = CreateValidJob();

        // Assert
        job.Status.Should().Be(JobStatus.Found);
    }

    [Fact]
    public void Job_DefaultConstructor_ShouldSetCreatedAtToRecentTime()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var job = CreateValidJob();

        // Assert
        var afterCreation = DateTime.UtcNow.AddSeconds(1);
        job.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        job.CreatedAt.Should().BeOnOrBefore(afterCreation);
    }

    [Fact]
    public void Job_DefaultConstructor_ShouldSetFoundAtToRecentTime()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var job = CreateValidJob();

        // Assert
        var afterCreation = DateTime.UtcNow.AddSeconds(1);
        job.FoundAt.Should().BeOnOrAfter(beforeCreation);
        job.FoundAt.Should().BeOnOrBefore(afterCreation);
    }

    [Fact]
    public void Job_DefaultConstructor_ShouldSetUpdatedAtToRecentTime()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var job = CreateValidJob();

        // Assert
        var afterCreation = DateTime.UtcNow.AddSeconds(1);
        job.UpdatedAt.Should().BeOnOrAfter(beforeCreation);
        job.UpdatedAt.Should().BeOnOrBefore(afterCreation);
    }

    [Fact]
    public void Job_DefaultConstructor_ShouldInitializeBooleanPropertiesToFalse()
    {
        // Arrange & Act
        var job = CreateValidJob();

        // Assert
        job.IsRemote.Should().BeFalse();
        job.IsContract.Should().BeFalse();
        job.IsTopApplicant.Should().BeFalse();
        job.IsPromoted.Should().BeFalse();
    }

    [Fact]
    public void Job_DefaultConstructor_ShouldInitializeScoreToZero()
    {
        // Arrange & Act
        var job = CreateValidJob();

        // Assert
        job.Score.Should().Be(0);
    }

    #endregion

    #region Required Properties Validation Tests

    [Fact]
    public void Job_LinkedInJobId_ShouldBeRequired()
    {
        // Arrange
        var job = CreateValidJob();
        job.LinkedInJobId = null!;

        // Act
        var validationResults = ValidateModel(job);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains("LinkedInJobId"));
    }

    [Fact]
    public void Job_Title_ShouldBeRequired()
    {
        // Arrange
        var job = CreateValidJob();
        job.Title = null!;

        // Act
        var validationResults = ValidateModel(job);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains("Title"));
    }

    [Fact]
    public void Job_Company_ShouldBeRequired()
    {
        // Arrange
        var job = CreateValidJob();
        job.Company = null!;

        // Act
        var validationResults = ValidateModel(job);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains("Company"));
    }

    [Fact]
    public void Job_Url_ShouldBeRequired()
    {
        // Arrange
        var job = CreateValidJob();
        job.Url = null!;

        // Act
        var validationResults = ValidateModel(job);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains("Url"));
    }

    #endregion

    #region StringLength Validation Tests

    [Theory]
    [InlineData(100, true)]   // At limit
    [InlineData(101, false)]  // Over limit
    [InlineData(50, true)]    // Under limit
    public void Job_LinkedInJobId_ShouldValidateStringLength(int length, bool isValid)
    {
        // Arrange
        var job = CreateValidJob();
        job.LinkedInJobId = new string('a', length);

        // Act
        var validationResults = ValidateModel(job);

        // Assert
        if (isValid)
        {
            validationResults.Should().NotContain(r => r.MemberNames.Contains("LinkedInJobId"));
        }
        else
        {
            validationResults.Should().Contain(r => r.MemberNames.Contains("LinkedInJobId"));
        }
    }

    [Theory]
    [InlineData(500, true)]   // At limit
    [InlineData(501, false)]  // Over limit
    [InlineData(100, true)]   // Under limit
    public void Job_Title_ShouldValidateStringLength(int length, bool isValid)
    {
        // Arrange
        var job = CreateValidJob();
        job.Title = new string('a', length);

        // Act
        var validationResults = ValidateModel(job);

        // Assert
        if (isValid)
        {
            validationResults.Should().NotContain(r => r.MemberNames.Contains("Title"));
        }
        else
        {
            validationResults.Should().Contain(r => r.MemberNames.Contains("Title"));
        }
    }

    [Theory]
    [InlineData(500, true)]   // At limit
    [InlineData(501, false)]  // Over limit
    [InlineData(100, true)]   // Under limit
    public void Job_Company_ShouldValidateStringLength(int length, bool isValid)
    {
        // Arrange
        var job = CreateValidJob();
        job.Company = new string('a', length);

        // Act
        var validationResults = ValidateModel(job);

        // Assert
        if (isValid)
        {
            validationResults.Should().NotContain(r => r.MemberNames.Contains("Company"));
        }
        else
        {
            validationResults.Should().Contain(r => r.MemberNames.Contains("Company"));
        }
    }

    [Theory]
    [InlineData(500, true)]   // At limit
    [InlineData(501, false)]  // Over limit
    public void Job_Location_ShouldValidateStringLength(int length, bool isValid)
    {
        // Arrange
        var job = CreateValidJob();
        job.Location = new string('a', length);

        // Act
        var validationResults = ValidateModel(job);

        // Assert
        if (isValid)
        {
            validationResults.Should().NotContain(r => r.MemberNames.Contains("Location"));
        }
        else
        {
            validationResults.Should().Contain(r => r.MemberNames.Contains("Location"));
        }
    }

    [Theory]
    [InlineData(2000, true)]  // At limit
    [InlineData(2001, false)] // Over limit
    public void Job_Url_ShouldValidateStringLength(int length, bool isValid)
    {
        // Arrange
        var job = CreateValidJob();
        job.Url = $"https://example.com/{new string('a', length - 20)}";

        // Act
        var validationResults = ValidateModel(job);

        // Assert
        if (isValid)
        {
            validationResults.Should().NotContain(r => r.MemberNames.Contains("Url"));
        }
        else
        {
            validationResults.Should().Contain(r => r.MemberNames.Contains("Url"));
        }
    }

    #endregion

    #region URL Validation Tests

    [Theory]
    [InlineData("https://linkedin.com/jobs/view/123", true)]
    [InlineData("http://example.com/job", true)]
    [InlineData("invalid-url", false)]
    [InlineData("not a url", false)]
    [InlineData("", false)]
    public void Job_Url_ShouldValidateUrlFormat(string url, bool isValid)
    {
        // Arrange
        var job = CreateValidJob();
        job.Url = url;

        // Act
        var validationResults = ValidateModel(job);

        // Assert
        if (isValid)
        {
            validationResults.Should().NotContain(r => r.MemberNames.Contains("Url") && 
                r.ErrorMessage != null && r.ErrorMessage.Contains("URL"));
        }
        else
        {
            validationResults.Should().Contain(r => r.MemberNames.Contains("Url"));
        }
    }

    [Theory]
    [InlineData("https://example.com/logo.png", true)]
    [InlineData("http://example.com/logo.jpg", true)]
    [InlineData("invalid-url", false)]
    [InlineData(null, true)] // Nullable, so null should be valid
    public void Job_CompanyLogoUrl_ShouldValidateUrlFormat(string? logoUrl, bool isValid)
    {
        // Arrange
        var job = CreateValidJob();
        job.CompanyLogoUrl = logoUrl;

        // Act
        var validationResults = ValidateModel(job);

        // Assert
        if (isValid)
        {
            validationResults.Should().NotContain(r => r.MemberNames.Contains("CompanyLogoUrl"));
        }
        else
        {
            validationResults.Should().Contain(r => r.MemberNames.Contains("CompanyLogoUrl"));
        }
    }

    #endregion

    #region Score Range Validation Tests

    [Theory]
    [InlineData(0, true)]     // Minimum boundary
    [InlineData(100, true)]   // Maximum boundary
    [InlineData(50, true)]    // Middle value
    [InlineData(-1, false)]   // Below minimum
    [InlineData(101, false)]  // Above maximum
    public void Job_Score_ShouldValidateRange(int score, bool isValid)
    {
        // Arrange
        var job = CreateValidJob();
        job.Score = score;

        // Act
        var validationResults = ValidateModel(job);

        // Assert
        if (isValid)
        {
            validationResults.Should().NotContain(r => r.MemberNames.Contains("Score"));
        }
        else
        {
            validationResults.Should().Contain(r => r.MemberNames.Contains("Score"));
        }
    }

    #endregion

    #region Nullable Properties Tests

    [Fact]
    public void Job_Location_CanBeNullWithoutValidationErrors()
    {
        // Arrange
        var job = CreateValidJob();
        job.Location = null;

        // Act
        var validationResults = ValidateModel(job);

        // Assert
        validationResults.Should().NotContain(r => r.MemberNames.Contains("Location"));
    }

    [Fact]
    public void Job_PayRate_CanBeNullWithoutValidationErrors()
    {
        // Arrange
        var job = CreateValidJob();
        job.PayRate = null;

        // Act
        var validationResults = ValidateModel(job);

        // Assert
        validationResults.Should().NotContain(r => r.MemberNames.Contains("PayRate"));
    }

    [Fact]
    public void Job_Salary_CanBeNullWithoutValidationErrors()
    {
        // Arrange
        var job = CreateValidJob();
        job.Salary = null;

        // Act
        var validationResults = ValidateModel(job);

        // Assert
        validationResults.Should().NotContain(r => r.MemberNames.Contains("Salary"));
    }

    [Fact]
    public void Job_JobType_CanBeNullWithoutValidationErrors()
    {
        // Arrange
        var job = CreateValidJob();
        job.JobType = null;

        // Act
        var validationResults = ValidateModel(job);

        // Assert
        validationResults.Should().NotContain(r => r.MemberNames.Contains("JobType"));
    }

    [Fact]
    public void Job_Description_CanBeNullWithoutValidationErrors()
    {
        // Arrange
        var job = CreateValidJob();
        job.Description = null;

        // Act
        var validationResults = ValidateModel(job);

        // Assert
        validationResults.Should().NotContain(r => r.MemberNames.Contains("Description"));
    }

    [Fact]
    public void Job_CompanyLogoUrl_CanBeNullWithoutValidationErrors()
    {
        // Arrange
        var job = CreateValidJob();
        job.CompanyLogoUrl = null;

        // Act
        var validationResults = ValidateModel(job);

        // Assert
        validationResults.Should().NotContain(r => r.MemberNames.Contains("CompanyLogoUrl"));
    }

    [Fact]
    public void Job_PostedDate_CanBeNull()
    {
        // Arrange
        var job = CreateValidJob();
        job.PostedDate = null;

        // Assert
        job.PostedDate.Should().BeNull();
    }

    [Fact]
    public void Job_AppliedAt_CanBeNull()
    {
        // Arrange
        var job = CreateValidJob();
        job.AppliedAt = null;

        // Assert
        job.AppliedAt.Should().BeNull();
    }

    #endregion

    #region Complete Job Instance Tests

    [Fact]
    public void Job_CanCreateValidInstanceWithAllProperties()
    {
        // Arrange
        var expectedId = Guid.NewGuid();
        var expectedFoundAt = DateTime.UtcNow.AddDays(-1);
        var expectedPostedDate = DateTime.UtcNow.AddDays(-7);
        var expectedAppliedAt = DateTime.UtcNow.AddHours(-2);
        var expectedCreatedAt = DateTime.UtcNow.AddDays(-1);
        var expectedUpdatedAt = DateTime.UtcNow;

        // Act
        var job = new Job
        {
            Id = expectedId,
            LinkedInJobId = "LI-12345",
            Title = "Senior Software Engineer",
            Company = "Tech Corp",
            Location = "San Francisco, CA",
            IsRemote = true,
            IsContract = false,
            PayRate = "$100/hr",
            Salary = "$200,000 - $250,000",
            JobType = "Full-time",
            IsTopApplicant = true,
            IsPromoted = false,
            Description = "A great job opportunity",
            Url = "https://linkedin.com/jobs/view/12345",
            CompanyLogoUrl = "https://example.com/logo.png",
            Score = 85,
            Status = JobStatus.Scored,
            FoundAt = expectedFoundAt,
            PostedDate = expectedPostedDate,
            AppliedAt = expectedAppliedAt,
            CreatedAt = expectedCreatedAt,
            UpdatedAt = expectedUpdatedAt
        };

        // Assert
        job.Id.Should().Be(expectedId);
        job.LinkedInJobId.Should().Be("LI-12345");
        job.Title.Should().Be("Senior Software Engineer");
        job.Company.Should().Be("Tech Corp");
        job.Location.Should().Be("San Francisco, CA");
        job.IsRemote.Should().BeTrue();
        job.IsContract.Should().BeFalse();
        job.PayRate.Should().Be("$100/hr");
        job.Salary.Should().Be("$200,000 - $250,000");
        job.JobType.Should().Be("Full-time");
        job.IsTopApplicant.Should().BeTrue();
        job.IsPromoted.Should().BeFalse();
        job.Description.Should().Be("A great job opportunity");
        job.Url.Should().Be("https://linkedin.com/jobs/view/12345");
        job.CompanyLogoUrl.Should().Be("https://example.com/logo.png");
        job.Score.Should().Be(85);
        job.Status.Should().Be(JobStatus.Scored);
        job.FoundAt.Should().Be(expectedFoundAt);
        job.PostedDate.Should().Be(expectedPostedDate);
        job.AppliedAt.Should().Be(expectedAppliedAt);
        job.CreatedAt.Should().Be(expectedCreatedAt);
        job.UpdatedAt.Should().Be(expectedUpdatedAt);
    }

    [Fact]
    public void Job_ValidInstance_ShouldPassAllValidations()
    {
        // Arrange
        var job = new Job
        {
            LinkedInJobId = "LI-12345",
            Title = "Senior Software Engineer",
            Company = "Tech Corp",
            Url = "https://linkedin.com/jobs/view/12345",
            Score = 85
        };

        // Act
        var validationResults = ValidateModel(job);

        // Assert
        validationResults.Should().BeEmpty("because the job instance should be valid");
    }

    #endregion

    #region Status Transitions Tests

    [Theory]
    [InlineData(JobStatus.Found)]
    [InlineData(JobStatus.Saved)]
    [InlineData(JobStatus.Scored)]
    [InlineData(JobStatus.Pending)]
    [InlineData(JobStatus.Applied)]
    [InlineData(JobStatus.Interviewing)]
    [InlineData(JobStatus.Offered)]
    [InlineData(JobStatus.Archived)]
    [InlineData(JobStatus.Rejected)]
    public void Job_CanSetAllStatusValues(JobStatus status)
    {
        // Arrange
        var job = CreateValidJob();

        // Act
        job.Status = status;

        // Assert
        job.Status.Should().Be(status);
    }

    #endregion

    #region Helper Methods

    private static Job CreateValidJob()
    {
        return new Job
        {
            LinkedInJobId = "TEST-123",
            Title = "Test Job",
            Company = "Test Company",
            Url = "https://linkedin.com/jobs/view/test"
        };
    }

    private static List<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model);
        Validator.TryValidateObject(model, validationContext, validationResults, validateAllProperties: true);
        return validationResults;
    }

    #endregion
}
