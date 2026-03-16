namespace JobCommandCenter.UnitTests.Shared;

/// <summary>
/// Unit tests for the Job domain model.
/// </summary>
public class JobTests
{
    #region Default Values Tests

    [Fact]
    public void Job_ShouldInitializeWithDefaultId()
    {
        // Arrange & Act
        var job = new Job
        {
            LinkedInJobId = "test-123",
            Title = "Software Engineer",
            Company = "Test Company",
            Url = "https://linkedin.com/jobs/test"
        };

        // Assert
        job.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Job_ShouldInitializeWithDefaultStatus()
    {
        // Arrange & Act
        var job = new Job
        {
            LinkedInJobId = "test-123",
            Title = "Software Engineer",
            Company = "Test Company",
            Url = "https://linkedin.com/jobs/test"
        };

        // Assert
        job.Status.Should().Be(JobStatus.Found);
    }

    [Fact]
    public void Job_ShouldInitializeWithDefaultScore()
    {
        // Arrange & Act
        var job = new Job
        {
            LinkedInJobId = "test-123",
            Title = "Software Engineer",
            Company = "Test Company",
            Url = "https://linkedin.com/jobs/test"
        };

        // Assert
        job.Score.Should().Be(0);
    }

    [Fact]
    public void Job_ShouldInitializeWithDefaultCreatedAt()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var job = new Job
        {
            LinkedInJobId = "test-123",
            Title = "Software Engineer",
            Company = "Test Company",
            Url = "https://linkedin.com/jobs/test"
        };
        var afterCreation = DateTime.UtcNow.AddSeconds(1);

        // Assert
        job.CreatedAt.Should().BeAfter(beforeCreation);
        job.CreatedAt.Should().BeBefore(afterCreation);
    }

    [Fact]
    public void Job_ShouldInitializeWithDefaultUpdatedAt()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var job = new Job
        {
            LinkedInJobId = "test-123",
            Title = "Software Engineer",
            Company = "Test Company",
            Url = "https://linkedin.com/jobs/test"
        };
        var afterCreation = DateTime.UtcNow.AddSeconds(1);

        // Assert
        job.UpdatedAt.Should().BeAfter(beforeCreation);
        job.UpdatedAt.Should().BeBefore(afterCreation);
    }

    [Fact]
    public void Job_ShouldInitializeBooleanFlagsAsFalse()
    {
        // Arrange & Act
        var job = new Job
        {
            LinkedInJobId = "test-123",
            Title = "Software Engineer",
            Company = "Test Company",
            Url = "https://linkedin.com/jobs/test"
        };

        // Assert
        job.IsRemote.Should().BeFalse();
        job.IsContract.Should().BeFalse();
        job.IsTopApplicant.Should().BeFalse();
        job.IsPromoted.Should().BeFalse();
    }

    #endregion

    #region Required Property Validation Tests

    [Fact]
    public void Job_LinkedInJobId_ShouldBeRequired()
    {
        // Arrange
        var job = new Job
        {
            LinkedInJobId = null!,
            Title = "Software Engineer",
            Company = "Test Company",
            Url = "https://linkedin.com/jobs/test"
        };

        // Act
        var validationResults = ValidateModel(job);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains(nameof(Job.LinkedInJobId)));
    }

    [Fact]
    public void Job_Title_ShouldBeRequired()
    {
        // Arrange
        var job = new Job
        {
            LinkedInJobId = "test-123",
            Title = null!,
            Company = "Test Company",
            Url = "https://linkedin.com/jobs/test"
        };

        // Act
        var validationResults = ValidateModel(job);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains(nameof(Job.Title)));
    }

    [Fact]
    public void Job_Company_ShouldBeRequired()
    {
        // Arrange
        var job = new Job
        {
            LinkedInJobId = "test-123",
            Title = "Software Engineer",
            Company = null!,
            Url = "https://linkedin.com/jobs/test"
        };

        // Act
        var validationResults = ValidateModel(job);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains(nameof(Job.Company)));
    }

    [Fact]
    public void Job_Url_ShouldBeRequired()
    {
        // Arrange
        var job = new Job
        {
            LinkedInJobId = "test-123",
            Title = "Software Engineer",
            Company = "Test Company",
            Url = null!
        };

        // Act
        var validationResults = ValidateModel(job);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains(nameof(Job.Url)));
    }

    #endregion

    #region StringLength Validation Tests

    [Fact]
    public void Job_LinkedInJobId_ShouldHaveMaxLength100()
    {
        // Arrange
        var job = new Job
        {
            LinkedInJobId = new string('a', 101),
            Title = "Software Engineer",
            Company = "Test Company",
            Url = "https://linkedin.com/jobs/test"
        };

        // Act
        var validationResults = ValidateModel(job);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains(nameof(Job.LinkedInJobId)));
    }

    [Fact]
    public void Job_Title_ShouldHaveMaxLength500()
    {
        // Arrange
        var job = new Job
        {
            LinkedInJobId = "test-123",
            Title = new string('a', 501),
            Company = "Test Company",
            Url = "https://linkedin.com/jobs/test"
        };

        // Act
        var validationResults = ValidateModel(job);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains(nameof(Job.Title)));
    }

    [Fact]
    public void Job_Company_ShouldHaveMaxLength500()
    {
        // Arrange
        var job = new Job
        {
            LinkedInJobId = "test-123",
            Title = "Software Engineer",
            Company = new string('a', 501),
            Url = "https://linkedin.com/jobs/test"
        };

        // Act
        var validationResults = ValidateModel(job);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains(nameof(Job.Company)));
    }

    [Fact]
    public void Job_Location_ShouldHaveMaxLength500()
    {
        // Arrange
        var job = new Job
        {
            LinkedInJobId = "test-123",
            Title = "Software Engineer",
            Company = "Test Company",
            Url = "https://linkedin.com/jobs/test",
            Location = new string('a', 501)
        };

        // Act
        var validationResults = ValidateModel(job);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains(nameof(Job.Location)));
    }

    [Fact]
    public void Job_PayRate_ShouldHaveMaxLength200()
    {
        // Arrange
        var job = new Job
        {
            LinkedInJobId = "test-123",
            Title = "Software Engineer",
            Company = "Test Company",
            Url = "https://linkedin.com/jobs/test",
            PayRate = new string('a', 201)
        };

        // Act
        var validationResults = ValidateModel(job);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains(nameof(Job.PayRate)));
    }

    [Fact]
    public void Job_Url_ShouldHaveMaxLength2000()
    {
        // Arrange
        var job = new Job
        {
            LinkedInJobId = "test-123",
            Title = "Software Engineer",
            Company = "Test Company",
            Url = "https://example.com/" + new string('a', 2000)
        };

        // Act
        var validationResults = ValidateModel(job);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains(nameof(Job.Url)));
    }

    #endregion

    #region Url Validation Tests

    [Fact]
    public void Job_Url_ShouldValidateUrlFormat()
    {
        // Arrange
        var job = new Job
        {
            LinkedInJobId = "test-123",
            Title = "Software Engineer",
            Company = "Test Company",
            Url = "not-a-valid-url"
        };

        // Act
        var validationResults = ValidateModel(job);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains(nameof(Job.Url)));
    }

    [Fact]
    public void Job_Url_ShouldAcceptValidUrl()
    {
        // Arrange
        var job = new Job
        {
            LinkedInJobId = "test-123",
            Title = "Software Engineer",
            Company = "Test Company",
            Url = "https://www.linkedin.com/jobs/view/123456"
        };

        // Act
        var validationResults = ValidateModel(job);

        // Assert
        validationResults.Should().NotContain(r => r.MemberNames.Contains(nameof(Job.Url)));
    }

    [Fact]
    public void Job_CompanyLogoUrl_ShouldValidateUrlFormat()
    {
        // Arrange
        var job = new Job
        {
            LinkedInJobId = "test-123",
            Title = "Software Engineer",
            Company = "Test Company",
            Url = "https://linkedin.com/jobs/test",
            CompanyLogoUrl = "not-a-valid-url"
        };

        // Act
        var validationResults = ValidateModel(job);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains(nameof(Job.CompanyLogoUrl)));
    }

    [Fact]
    public void Job_CompanyLogoUrl_ShouldAcceptValidUrl()
    {
        // Arrange
        var job = new Job
        {
            LinkedInJobId = "test-123",
            Title = "Software Engineer",
            Company = "Test Company",
            Url = "https://linkedin.com/jobs/test",
            CompanyLogoUrl = "https://media.licdn.com/dms/image/logo.png"
        };

        // Act
        var validationResults = ValidateModel(job);

        // Assert
        validationResults.Should().NotContain(r => r.MemberNames.Contains(nameof(Job.CompanyLogoUrl)));
    }

    [Fact]
    public void Job_CompanyLogoUrl_ShouldHaveMaxLength2000()
    {
        // Arrange
        var job = new Job
        {
            LinkedInJobId = "test-123",
            Title = "Software Engineer",
            Company = "Test Company",
            Url = "https://linkedin.com/jobs/test",
            CompanyLogoUrl = "https://example.com/" + new string('a', 2000)
        };

        // Act
        var validationResults = ValidateModel(job);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains(nameof(Job.CompanyLogoUrl)));
    }

    #endregion

    #region Nullable Properties Tests

    [Fact]
    public void Job_Location_CanBeNull()
    {
        // Arrange & Act
        var job = new Job
        {
            LinkedInJobId = "test-123",
            Title = "Software Engineer",
            Company = "Test Company",
            Url = "https://linkedin.com/jobs/test",
            Location = null
        };

        // Assert
        job.Location.Should().BeNull();
    }

    [Fact]
    public void Job_PayRate_CanBeNull()
    {
        // Arrange & Act
        var job = new Job
        {
            LinkedInJobId = "test-123",
            Title = "Software Engineer",
            Company = "Test Company",
            Url = "https://linkedin.com/jobs/test",
            PayRate = null
        };

        // Assert
        job.PayRate.Should().BeNull();
    }

    [Fact]
    public void Job_Salary_CanBeNull()
    {
        // Arrange & Act
        var job = new Job
        {
            LinkedInJobId = "test-123",
            Title = "Software Engineer",
            Company = "Test Company",
            Url = "https://linkedin.com/jobs/test",
            Salary = null
        };

        // Assert
        job.Salary.Should().BeNull();
    }

    [Fact]
    public void Job_JobType_CanBeNull()
    {
        // Arrange & Act
        var job = new Job
        {
            LinkedInJobId = "test-123",
            Title = "Software Engineer",
            Company = "Test Company",
            Url = "https://linkedin.com/jobs/test",
            JobType = null
        };

        // Assert
        job.JobType.Should().BeNull();
    }

    [Fact]
    public void Job_Description_CanBeNull()
    {
        // Arrange & Act
        var job = new Job
        {
            LinkedInJobId = "test-123",
            Title = "Software Engineer",
            Company = "Test Company",
            Url = "https://linkedin.com/jobs/test",
            Description = null
        };

        // Assert
        job.Description.Should().BeNull();
    }

    [Fact]
    public void Job_CompanyLogoUrl_CanBeNull()
    {
        // Arrange & Act
        var job = new Job
        {
            LinkedInJobId = "test-123",
            Title = "Software Engineer",
            Company = "Test Company",
            Url = "https://linkedin.com/jobs/test",
            CompanyLogoUrl = null
        };

        // Assert
        job.CompanyLogoUrl.Should().BeNull();
    }

    [Fact]
    public void Job_PostedDate_CanBeNull()
    {
        // Arrange & Act
        var job = new Job
        {
            LinkedInJobId = "test-123",
            Title = "Software Engineer",
            Company = "Test Company",
            Url = "https://linkedin.com/jobs/test",
            PostedDate = null
        };

        // Assert
        job.PostedDate.Should().BeNull();
    }

    [Fact]
    public void Job_AppliedAt_CanBeNull()
    {
        // Arrange & Act
        var job = new Job
        {
            LinkedInJobId = "test-123",
            Title = "Software Engineer",
            Company = "Test Company",
            Url = "https://linkedin.com/jobs/test",
            AppliedAt = null
        };

        // Assert
        job.AppliedAt.Should().BeNull();
    }

    #endregion

    #region New Properties Existence Tests

    [Fact]
    public void Job_ShouldHaveSalaryProperty()
    {
        // Arrange & Act
        var job = new Job
        {
            LinkedInJobId = "test-123",
            Title = "Software Engineer",
            Company = "Test Company",
            Url = "https://linkedin.com/jobs/test",
            Salary = "$100,000 - $150,000"
        };

        // Assert
        job.Salary.Should().Be("$100,000 - $150,000");
    }

    [Fact]
    public void Job_ShouldHaveJobTypeProperty()
    {
        // Arrange & Act
        var job = new Job
        {
            LinkedInJobId = "test-123",
            Title = "Software Engineer",
            Company = "Test Company",
            Url = "https://linkedin.com/jobs/test",
            JobType = "Full-time"
        };

        // Assert
        job.JobType.Should().Be("Full-time");
    }

    [Fact]
    public void Job_ShouldHaveAppliedAtProperty()
    {
        // Arrange
        var appliedDate = new DateTime(2025, 3, 15, 10, 30, 0, DateTimeKind.Utc);

        // Act
        var job = new Job
        {
            LinkedInJobId = "test-123",
            Title = "Software Engineer",
            Company = "Test Company",
            Url = "https://linkedin.com/jobs/test",
            AppliedAt = appliedDate
        };

        // Assert
        job.AppliedAt.Should().Be(appliedDate);
    }

    [Fact]
    public void Job_ShouldHaveCompanyLogoUrlProperty()
    {
        // Arrange & Act
        var job = new Job
        {
            LinkedInJobId = "test-123",
            Title = "Software Engineer",
            Company = "Test Company",
            Url = "https://linkedin.com/jobs/test",
            CompanyLogoUrl = "https://media.licdn.com/company/logo.png"
        };

        // Assert
        job.CompanyLogoUrl.Should().Be("https://media.licdn.com/company/logo.png");
    }

    [Fact]
    public void Job_ShouldHavePostedDateProperty()
    {
        // Arrange
        var postedDate = new DateTime(2025, 3, 10, 8, 0, 0, DateTimeKind.Utc);

        // Act
        var job = new Job
        {
            LinkedInJobId = "test-123",
            Title = "Software Engineer",
            Company = "Test Company",
            Url = "https://linkedin.com/jobs/test",
            PostedDate = postedDate
        };

        // Assert
        job.PostedDate.Should().Be(postedDate);
    }

    [Fact]
    public void Job_Salary_ShouldHaveMaxLength200()
    {
        // Arrange
        var job = new Job
        {
            LinkedInJobId = "test-123",
            Title = "Software Engineer",
            Company = "Test Company",
            Url = "https://linkedin.com/jobs/test",
            Salary = new string('a', 201)
        };

        // Act
        var validationResults = ValidateModel(job);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains(nameof(Job.Salary)));
    }

    [Fact]
    public void Job_JobType_ShouldHaveMaxLength100()
    {
        // Arrange
        var job = new Job
        {
            LinkedInJobId = "test-123",
            Title = "Software Engineer",
            Company = "Test Company",
            Url = "https://linkedin.com/jobs/test",
            JobType = new string('a', 101)
        };

        // Act
        var validationResults = ValidateModel(job);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains(nameof(Job.JobType)));
    }

    #endregion

    #region Complete Job Tests

    [Fact]
    public void Job_ShouldBeValid_WhenAllRequiredPropertiesAreSet()
    {
        // Arrange
        var job = new Job
        {
            LinkedInJobId = "LI-123456",
            Title = "Senior Software Engineer",
            Company = "Tech Corp",
            Url = "https://linkedin.com/jobs/view/123456"
        };

        // Act
        var validationResults = ValidateModel(job);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void Job_ShouldAllowAllPropertiesToBeSet()
    {
        // Arrange
        var id = Guid.NewGuid();
        var postedDate = new DateTime(2025, 3, 1, 12, 0, 0, DateTimeKind.Utc);
        var appliedAt = new DateTime(2025, 3, 5, 14, 30, 0, DateTimeKind.Utc);
        var createdAt = new DateTime(2025, 3, 1, 10, 0, 0, DateTimeKind.Utc);
        var updatedAt = new DateTime(2025, 3, 5, 14, 30, 0, DateTimeKind.Utc);

        // Act
        var job = new Job
        {
            Id = id,
            LinkedInJobId = "LI-789",
            Title = "Lead Developer",
            Company = "Startup Inc",
            Location = "San Francisco, CA",
            IsRemote = true,
            IsContract = false,
            PayRate = "$80/hr",
            Salary = "$180,000 - $220,000",
            JobType = "Full-time",
            IsTopApplicant = true,
            IsPromoted = false,
            Description = "A great opportunity...",
            Url = "https://linkedin.com/jobs/view/789",
            CompanyLogoUrl = "https://media.licdn.com/startup-logo.png",
            Score = 85,
            Status = JobStatus.Applied,
            PostedDate = postedDate,
            AppliedAt = appliedAt,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        // Assert
        job.Id.Should().Be(id);
        job.LinkedInJobId.Should().Be("LI-789");
        job.Title.Should().Be("Lead Developer");
        job.Company.Should().Be("Startup Inc");
        job.Location.Should().Be("San Francisco, CA");
        job.IsRemote.Should().BeTrue();
        job.IsContract.Should().BeFalse();
        job.PayRate.Should().Be("$80/hr");
        job.Salary.Should().Be("$180,000 - $220,000");
        job.JobType.Should().Be("Full-time");
        job.IsTopApplicant.Should().BeTrue();
        job.IsPromoted.Should().BeFalse();
        job.Description.Should().Be("A great opportunity...");
        job.Url.Should().Be("https://linkedin.com/jobs/view/789");
        job.CompanyLogoUrl.Should().Be("https://media.licdn.com/startup-logo.png");
        job.Score.Should().Be(85);
        job.Status.Should().Be(JobStatus.Applied);
        job.PostedDate.Should().Be(postedDate);
        job.AppliedAt.Should().Be(appliedAt);
        job.CreatedAt.Should().Be(createdAt);
        job.UpdatedAt.Should().Be(updatedAt);
    }

    #endregion

    #region Helper Methods

    private static List<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model);
        Validator.TryValidateObject(model, validationContext, validationResults, validateAllProperties: true);
        return validationResults;
    }

    #endregion
}
