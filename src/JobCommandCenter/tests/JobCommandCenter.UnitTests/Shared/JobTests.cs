using FluentAssertions;
using JobCommandCenter.Shared.Models;

namespace JobCommandCenter.UnitTests.Shared;

public class JobTests
{
    #region Constructor and Default Values

    [Fact]
    public void Job_Should_InitializeWithDefaultId()
    {
        // Arrange & Act
        var job = CreateValidJob();

        // Assert
        job.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Job_Should_InitializeWithDefaultStatusFound()
    {
        // Arrange & Act
        var job = CreateValidJob();

        // Assert
        job.Status.Should().Be(JobStatus.Found);
    }

    [Fact]
    public void Job_Should_InitializeWithDefaultScoreZero()
    {
        // Arrange & Act
        var job = CreateValidJob();

        // Assert
        job.Score.Should().Be(0);
    }

    [Fact]
    public void Job_Should_InitializeWithCurrentUtcTime()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var job = CreateValidJob();

        // Assert
        var afterCreation = DateTime.UtcNow.AddSeconds(1);
        job.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        job.CreatedAt.Should().BeOnOrBefore(afterCreation);
        job.UpdatedAt.Should().BeOnOrAfter(beforeCreation);
        job.UpdatedAt.Should().BeOnOrBefore(afterCreation);
    }

    [Fact]
    public void Job_Should_HaveSameCreatedAtAndUpdatedAtOnInitialization()
    {
        // Arrange & Act
        var job = CreateValidJob();

        // Assert
        job.CreatedAt.Should().BeCloseTo(job.UpdatedAt, TimeSpan.FromMilliseconds(100));
    }

    #endregion

    #region Required Properties

    [Fact]
    public void Job_Should_SetLinkedInJobId()
    {
        // Arrange & Act
        var job = new Job
        {
            LinkedInJobId = "LI-12345",
            Title = "Software Engineer",
            Company = "Tech Corp",
            Url = "https://linkedin.com/jobs/123"
        };

        // Assert
        job.LinkedInJobId.Should().Be("LI-12345");
    }

    [Fact]
    public void Job_Should_SetTitle()
    {
        // Arrange & Act
        var job = new Job
        {
            LinkedInJobId = "LI-12345",
            Title = "Senior Software Engineer",
            Company = "Tech Corp",
            Url = "https://linkedin.com/jobs/123"
        };

        // Assert
        job.Title.Should().Be("Senior Software Engineer");
    }

    [Fact]
    public void Job_Should_SetCompany()
    {
        // Arrange & Act
        var job = new Job
        {
            LinkedInJobId = "LI-12345",
            Title = "Software Engineer",
            Company = "Acme Corporation",
            Url = "https://linkedin.com/jobs/123"
        };

        // Assert
        job.Company.Should().Be("Acme Corporation");
    }

    [Fact]
    public void Job_Should_SetUrl()
    {
        // Arrange & Act
        var job = new Job
        {
            LinkedInJobId = "LI-12345",
            Title = "Software Engineer",
            Company = "Tech Corp",
            Url = "https://linkedin.com/jobs/view/12345"
        };

        // Assert
        job.Url.Should().Be("https://linkedin.com/jobs/view/12345");
    }

    #endregion

    #region Nullable Properties

    [Fact]
    public void Job_Should_AllowNullLocation()
    {
        // Arrange & Act
        var job = CreateValidJob();

        // Assert
        job.Location.Should().BeNull();
    }

    [Fact]
    public void Job_Should_SetLocation()
    {
        // Arrange & Act
        var job = CreateValidJob();
        job.Location = "San Francisco, CA";

        // Assert
        job.Location.Should().Be("San Francisco, CA");
    }

    [Fact]
    public void Job_Should_AllowNullPayRate()
    {
        // Arrange & Act
        var job = CreateValidJob();

        // Assert
        job.PayRate.Should().BeNull();
    }

    [Fact]
    public void Job_Should_SetPayRate()
    {
        // Arrange & Act
        var job = CreateValidJob();
        job.PayRate = "$150,000 - $200,000";

        // Assert
        job.PayRate.Should().Be("$150,000 - $200,000");
    }

    [Fact]
    public void Job_Should_AllowNullDescription()
    {
        // Arrange & Act
        var job = CreateValidJob();

        // Assert
        job.Description.Should().BeNull();
    }

    [Fact]
    public void Job_Should_SetDescription()
    {
        // Arrange & Act
        var job = CreateValidJob();
        job.Description = "We are looking for a talented software engineer...";

        // Assert
        job.Description.Should().Be("We are looking for a talented software engineer...");
    }

    #endregion

    #region Boolean Properties

    [Fact]
    public void Job_Should_DefaultIsRemoteToFalse()
    {
        // Arrange & Act
        var job = CreateValidJob();

        // Assert
        job.IsRemote.Should().BeFalse();
    }

    [Fact]
    public void Job_Should_SetIsRemoteToTrue()
    {
        // Arrange & Act
        var job = CreateValidJob();
        job.IsRemote = true;

        // Assert
        job.IsRemote.Should().BeTrue();
    }

    [Fact]
    public void Job_Should_DefaultIsContractToFalse()
    {
        // Arrange & Act
        var job = CreateValidJob();

        // Assert
        job.IsContract.Should().BeFalse();
    }

    [Fact]
    public void Job_Should_SetIsContractToTrue()
    {
        // Arrange & Act
        var job = CreateValidJob();
        job.IsContract = true;

        // Assert
        job.IsContract.Should().BeTrue();
    }

    [Fact]
    public void Job_Should_DefaultIsTopApplicantToFalse()
    {
        // Arrange & Act
        var job = CreateValidJob();

        // Assert
        job.IsTopApplicant.Should().BeFalse();
    }

    [Fact]
    public void Job_Should_SetIsTopApplicantToTrue()
    {
        // Arrange & Act
        var job = CreateValidJob();
        job.IsTopApplicant = true;

        // Assert
        job.IsTopApplicant.Should().BeTrue();
    }

    [Fact]
    public void Job_Should_DefaultIsPromotedToFalse()
    {
        // Arrange & Act
        var job = CreateValidJob();

        // Assert
        job.IsPromoted.Should().BeFalse();
    }

    [Fact]
    public void Job_Should_SetIsPromotedToTrue()
    {
        // Arrange & Act
        var job = CreateValidJob();
        job.IsPromoted = true;

        // Assert
        job.IsPromoted.Should().BeTrue();
    }

    #endregion

    #region Score and Status Properties

    [Fact]
    public void Job_Should_SetScore()
    {
        // Arrange & Act
        var job = CreateValidJob();
        job.Score = 85;

        // Assert
        job.Score.Should().Be(85);
    }

    [Fact]
    public void Job_Should_AllowNegativeScore()
    {
        // Arrange & Act
        var job = CreateValidJob();
        job.Score = -20;

        // Assert
        job.Score.Should().Be(-20);
    }

    [Fact]
    public void Job_Should_SetStatusToScored()
    {
        // Arrange & Act
        var job = CreateValidJob();
        job.Status = JobStatus.Scored;

        // Assert
        job.Status.Should().Be(JobStatus.Scored);
    }

    [Fact]
    public void Job_Should_SetStatusToPending()
    {
        // Arrange & Act
        var job = CreateValidJob();
        job.Status = JobStatus.Pending;

        // Assert
        job.Status.Should().Be(JobStatus.Pending);
    }

    [Fact]
    public void Job_Should_SetStatusToApplied()
    {
        // Arrange & Act
        var job = CreateValidJob();
        job.Status = JobStatus.Applied;

        // Assert
        job.Status.Should().Be(JobStatus.Applied);
    }

    [Fact]
    public void Job_Should_SetStatusToInterviewing()
    {
        // Arrange & Act
        var job = CreateValidJob();
        job.Status = JobStatus.Interviewing;

        // Assert
        job.Status.Should().Be(JobStatus.Interviewing);
    }

    [Fact]
    public void Job_Should_SetStatusToArchived()
    {
        // Arrange & Act
        var job = CreateValidJob();
        job.Status = JobStatus.Archived;

        // Assert
        job.Status.Should().Be(JobStatus.Archived);
    }

    [Fact]
    public void Job_Should_SetStatusToRejected()
    {
        // Arrange & Act
        var job = CreateValidJob();
        job.Status = JobStatus.Rejected;

        // Assert
        job.Status.Should().Be(JobStatus.Rejected);
    }

    #endregion

    #region Timestamp Properties

    [Fact]
    public void Job_Should_SetCreatedAt()
    {
        // Arrange
        var job = CreateValidJob();
        var customDate = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);

        // Act
        job.CreatedAt = customDate;

        // Assert
        job.CreatedAt.Should().Be(customDate);
    }

    [Fact]
    public void Job_Should_SetUpdatedAt()
    {
        // Arrange
        var job = CreateValidJob();
        var customDate = new DateTime(2024, 1, 20, 14, 45, 0, DateTimeKind.Utc);

        // Act
        job.UpdatedAt = customDate;

        // Assert
        job.UpdatedAt.Should().Be(customDate);
    }

    [Fact]
    public void Job_Should_AllowUpdatedAtToBeAfterCreatedAt()
    {
        // Arrange
        var job = CreateValidJob();
        job.CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        job.UpdatedAt = new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc);

        // Assert
        job.UpdatedAt.Should().BeAfter(job.CreatedAt);
    }

    #endregion

    #region Id Property

    [Fact]
    public void Job_Should_GenerateUniqueIds()
    {
        // Arrange & Act
        var job1 = CreateValidJob();
        var job2 = CreateValidJob();

        // Assert
        job1.Id.Should().NotBe(job2.Id);
    }

    [Fact]
    public void Job_Should_AllowCustomId()
    {
        // Arrange
        var customId = Guid.Parse("12345678-1234-1234-1234-123456789012");

        // Act
        var job = CreateValidJob();
        job.Id = customId;

        // Assert
        job.Id.Should().Be(customId);
    }

    #endregion

    #region Helper Methods

    private static Job CreateValidJob()
    {
        return new Job
        {
            LinkedInJobId = "LI-12345",
            Title = "Software Engineer",
            Company = "Tech Corp",
            Url = "https://linkedin.com/jobs/123"
        };
    }

    #endregion
}
