using FluentAssertions;
using JobCommandCenter.Shared.Models;

namespace JobCommandCenter.UnitTests.Shared;

public class JobStatusTests
{
    #region Enum Values Exist

    [Fact]
    public void JobStatus_Should_HaveFoundValue()
    {
        // Arrange & Act
        var status = JobStatus.Found;

        // Assert
        status.Should().BeDefined();
        status.Should().Be(JobStatus.Found);
    }

    [Fact]
    public void JobStatus_Should_HaveScoredValue()
    {
        // Arrange & Act
        var status = JobStatus.Scored;

        // Assert
        status.Should().BeDefined();
        status.Should().Be(JobStatus.Scored);
    }

    [Fact]
    public void JobStatus_Should_HavePendingValue()
    {
        // Arrange & Act
        var status = JobStatus.Pending;

        // Assert
        status.Should().BeDefined();
        status.Should().Be(JobStatus.Pending);
    }

    [Fact]
    public void JobStatus_Should_HaveAppliedValue()
    {
        // Arrange & Act
        var status = JobStatus.Applied;

        // Assert
        status.Should().BeDefined();
        status.Should().Be(JobStatus.Applied);
    }

    [Fact]
    public void JobStatus_Should_HaveInterviewingValue()
    {
        // Arrange & Act
        var status = JobStatus.Interviewing;

        // Assert
        status.Should().BeDefined();
        status.Should().Be(JobStatus.Interviewing);
    }

    [Fact]
    public void JobStatus_Should_HaveArchivedValue()
    {
        // Arrange & Act
        var status = JobStatus.Archived;

        // Assert
        status.Should().BeDefined();
        status.Should().Be(JobStatus.Archived);
    }

    [Fact]
    public void JobStatus_Should_HaveRejectedValue()
    {
        // Arrange & Act
        var status = JobStatus.Rejected;

        // Assert
        status.Should().BeDefined();
        status.Should().Be(JobStatus.Rejected);
    }

    #endregion

    #region Enum Value Ordering

    [Fact]
    public void JobStatus_Found_Should_HaveValueZero()
    {
        // Arrange & Act & Assert
        ((int)JobStatus.Found).Should().Be(0);
    }

    [Fact]
    public void JobStatus_Scored_Should_HaveValueOne()
    {
        // Arrange & Act & Assert
        ((int)JobStatus.Scored).Should().Be(1);
    }

    [Fact]
    public void JobStatus_Pending_Should_HaveValueTwo()
    {
        // Arrange & Act & Assert
        ((int)JobStatus.Pending).Should().Be(2);
    }

    [Fact]
    public void JobStatus_Applied_Should_HaveValueThree()
    {
        // Arrange & Act & Assert
        ((int)JobStatus.Applied).Should().Be(3);
    }

    [Fact]
    public void JobStatus_Interviewing_Should_HaveValueFour()
    {
        // Arrange & Act & Assert
        ((int)JobStatus.Interviewing).Should().Be(4);
    }

    [Fact]
    public void JobStatus_Archived_Should_HaveValueFive()
    {
        // Arrange & Act & Assert
        ((int)JobStatus.Archived).Should().Be(5);
    }

    [Fact]
    public void JobStatus_Rejected_Should_HaveValueSix()
    {
        // Arrange & Act & Assert
        ((int)JobStatus.Rejected).Should().Be(6);
    }

    [Fact]
    public void JobStatus_Should_HaveSequentialValues()
    {
        // Arrange
        var expectedValues = new[] { 0, 1, 2, 3, 4, 5, 6 };
        var actualValues = Enum.GetValues<JobStatus>().Cast<int>().OrderBy(v => v).ToArray();

        // Assert
        actualValues.Should().Equal(expectedValues);
    }

    [Fact]
    public void JobStatus_Should_HaveSevenValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<JobStatus>();

        // Assert
        values.Should().HaveCount(7);
    }

    #endregion

    #region Default Status

    [Fact]
    public void JobStatus_DefaultValue_ShouldBeFound()
    {
        // Arrange & Act
        JobStatus defaultStatus = default;

        // Assert
        defaultStatus.Should().Be(JobStatus.Found);
    }

    [Fact]
    public void JobStatus_NewJob_ShouldDefaultToFound()
    {
        // Arrange & Act
        var job = new Job
        {
            LinkedInJobId = "test-id",
            Title = "Test Job",
            Company = "Test Company",
            Url = "https://test.com"
        };

        // Assert
        job.Status.Should().Be(JobStatus.Found);
    }

    [Fact]
    public void JobStatus_ZeroCast_ShouldBeFound()
    {
        // Arrange & Act
        var status = (JobStatus)0;

        // Assert
        status.Should().Be(JobStatus.Found);
    }

    #endregion

    #region Enum Parsing

    [Fact]
    public void JobStatus_Should_ParseFromString()
    {
        // Arrange & Act
        var found = Enum.Parse<JobStatus>("Found");
        var scored = Enum.Parse<JobStatus>("Scored");
        var pending = Enum.Parse<JobStatus>("Pending");
        var applied = Enum.Parse<JobStatus>("Applied");
        var interviewing = Enum.Parse<JobStatus>("Interviewing");
        var archived = Enum.Parse<JobStatus>("Archived");
        var rejected = Enum.Parse<JobStatus>("Rejected");

        // Assert
        found.Should().Be(JobStatus.Found);
        scored.Should().Be(JobStatus.Scored);
        pending.Should().Be(JobStatus.Pending);
        applied.Should().Be(JobStatus.Applied);
        interviewing.Should().Be(JobStatus.Interviewing);
        archived.Should().Be(JobStatus.Archived);
        rejected.Should().Be(JobStatus.Rejected);
    }

    [Fact]
    public void JobStatus_Should_ConvertFromInt()
    {
        // Arrange & Act & Assert
        ((JobStatus)0).Should().Be(JobStatus.Found);
        ((JobStatus)1).Should().Be(JobStatus.Scored);
        ((JobStatus)2).Should().Be(JobStatus.Pending);
        ((JobStatus)3).Should().Be(JobStatus.Applied);
        ((JobStatus)4).Should().Be(JobStatus.Interviewing);
        ((JobStatus)5).Should().Be(JobStatus.Archived);
        ((JobStatus)6).Should().Be(JobStatus.Rejected);
    }

    [Fact]
    public void JobStatus_Should_GetAllNames()
    {
        // Arrange & Act
        var names = Enum.GetNames<JobStatus>();

        // Assert
        names.Should().Contain(new[]
        {
            "Found", "Scored", "Pending", "Applied", "Interviewing", "Archived", "Rejected"
        });
    }

    #endregion

    #region Status Transitions

    [Fact]
    public void JobStatus_Should_AllowTransitionFromFoundToScored()
    {
        // Arrange
        var job = new Job
        {
            LinkedInJobId = "test-id",
            Title = "Test Job",
            Company = "Test Company",
            Url = "https://test.com",
            Status = JobStatus.Found
        };

        // Act
        job.Status = JobStatus.Scored;

        // Assert
        job.Status.Should().Be(JobStatus.Scored);
    }

    [Fact]
    public void JobStatus_Should_AllowTransitionFromScoredToPending()
    {
        // Arrange
        var job = new Job
        {
            LinkedInJobId = "test-id",
            Title = "Test Job",
            Company = "Test Company",
            Url = "https://test.com",
            Status = JobStatus.Scored
        };

        // Act
        job.Status = JobStatus.Pending;

        // Assert
        job.Status.Should().Be(JobStatus.Pending);
    }

    [Fact]
    public void JobStatus_Should_AllowTransitionFromPendingToApplied()
    {
        // Arrange
        var job = new Job
        {
            LinkedInJobId = "test-id",
            Title = "Test Job",
            Company = "Test Company",
            Url = "https://test.com",
            Status = JobStatus.Pending
        };

        // Act
        job.Status = JobStatus.Applied;

        // Assert
        job.Status.Should().Be(JobStatus.Applied);
    }

    [Fact]
    public void JobStatus_Should_AllowTransitionFromAppliedToInterviewing()
    {
        // Arrange
        var job = new Job
        {
            LinkedInJobId = "test-id",
            Title = "Test Job",
            Company = "Test Company",
            Url = "https://test.com",
            Status = JobStatus.Applied
        };

        // Act
        job.Status = JobStatus.Interviewing;

        // Assert
        job.Status.Should().Be(JobStatus.Interviewing);
    }

    [Fact]
    public void JobStatus_Should_AllowTransitionToArchived()
    {
        // Arrange
        var job = new Job
        {
            LinkedInJobId = "test-id",
            Title = "Test Job",
            Company = "Test Company",
            Url = "https://test.com",
            Status = JobStatus.Applied
        };

        // Act
        job.Status = JobStatus.Archived;

        // Assert
        job.Status.Should().Be(JobStatus.Archived);
    }

    [Fact]
    public void JobStatus_Should_AllowTransitionToRejected()
    {
        // Arrange
        var job = new Job
        {
            LinkedInJobId = "test-id",
            Title = "Test Job",
            Company = "Test Company",
            Url = "https://test.com",
            Status = JobStatus.Pending
        };

        // Act
        job.Status = JobStatus.Rejected;

        // Assert
        job.Status.Should().Be(JobStatus.Rejected);
    }

    #endregion

    #region Comparison Operations

    [Fact]
    public void JobStatus_Should_CompareCorrectly()
    {
        // Assert
        (JobStatus.Found < JobStatus.Scored).Should().BeTrue();
        (JobStatus.Scored < JobStatus.Pending).Should().BeTrue();
        (JobStatus.Pending < JobStatus.Applied).Should().BeTrue();
        (JobStatus.Applied < JobStatus.Interviewing).Should().BeTrue();
        (JobStatus.Interviewing < JobStatus.Archived).Should().BeTrue();
        (JobStatus.Archived < JobStatus.Rejected).Should().BeTrue();
    }

    [Fact]
    public void JobStatus_Equality_ShouldWorkCorrectly()
    {
        // Assert
#pragma warning disable CS1718 // Comparison made to same variable - intentional test
        var found = JobStatus.Found;
        (found == JobStatus.Found).Should().BeTrue();
#pragma warning restore CS1718
        (JobStatus.Found != JobStatus.Scored).Should().BeTrue();
        (JobStatus.Applied != JobStatus.Interviewing).Should().BeTrue();
    }

    #endregion
}
