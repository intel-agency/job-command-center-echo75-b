using JobCommandCenter.Data.Entities;
using JobCommandCenter.Shared.Models;

namespace JobCommandCenter.UnitTests.Data;

/// <summary>
/// Unit tests for <see cref="JobEntity"/>.
/// </summary>
public class JobEntityTests
{
    [Fact]
    public void JobEntity_FromModel_ConvertsAllProperties()
    {
        // Arrange
        var model = new Job
        {
            Id = Guid.NewGuid(),
            LinkedInJobId = "linkedin-123",
            Title = "Senior Software Engineer",
            Company = "Tech Corp",
            Location = "Remote",
            IsRemote = true,
            IsContract = false,
            PayRate = "$150k-$200k",
            IsTopApplicant = true,
            IsPromoted = false,
            Description = "A great job opportunity",
            Url = "https://linkedin.com/jobs/123",
            Score = 85,
            Status = JobStatus.Saved,
            CreatedAt = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 1, 15, 11, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var entity = JobEntity.FromModel(model);

        // Assert
        entity.Id.Should().Be(model.Id);
        entity.LinkedInJobId.Should().Be(model.LinkedInJobId);
        entity.Title.Should().Be(model.Title);
        entity.Company.Should().Be(model.Company);
        entity.Location.Should().Be(model.Location);
        entity.IsRemote.Should().Be(model.IsRemote);
        entity.IsContract.Should().Be(model.IsContract);
        entity.PayRate.Should().Be(model.PayRate);
        entity.IsTopApplicant.Should().Be(model.IsTopApplicant);
        entity.IsPromoted.Should().Be(model.IsPromoted);
        entity.Description.Should().Be(model.Description);
        entity.Url.Should().Be(model.Url);
        entity.Score.Should().Be(model.Score);
        entity.Status.Should().Be(model.Status);
        entity.CreatedAt.Should().Be(model.CreatedAt);
        entity.UpdatedAt.Should().Be(model.UpdatedAt);
    }

    [Fact]
    public void JobEntity_ToModel_ConvertsAllProperties()
    {
        // Arrange
        var entity = new JobEntity
        {
            Id = Guid.NewGuid(),
            LinkedInJobId = "linkedin-456",
            Title = "Lead Developer",
            Company = "Startup Inc",
            Location = "New York, NY",
            IsRemote = false,
            IsContract = true,
            PayRate = "$100/hr",
            IsTopApplicant = false,
            IsPromoted = true,
            Description = "Join our team",
            Url = "https://linkedin.com/jobs/456",
            Score = 60,
            Status = JobStatus.Applied,
            CreatedAt = new DateTime(2024, 2, 1, 9, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 2, 1, 9, 30, 0, DateTimeKind.Utc)
        };

        // Act
        var model = entity.ToModel();

        // Assert
        model.Id.Should().Be(entity.Id);
        model.LinkedInJobId.Should().Be(entity.LinkedInJobId);
        model.Title.Should().Be(entity.Title);
        model.Company.Should().Be(entity.Company);
        model.Location.Should().Be(entity.Location);
        model.IsRemote.Should().Be(entity.IsRemote);
        model.IsContract.Should().Be(entity.IsContract);
        model.PayRate.Should().Be(entity.PayRate);
        model.IsTopApplicant.Should().Be(entity.IsTopApplicant);
        model.IsPromoted.Should().Be(entity.IsPromoted);
        model.Description.Should().Be(entity.Description);
        model.Url.Should().Be(entity.Url);
        model.Score.Should().Be(entity.Score);
        model.Status.Should().Be(entity.Status);
        model.CreatedAt.Should().Be(entity.CreatedAt);
        model.UpdatedAt.Should().Be(entity.UpdatedAt);
    }

    [Fact]
    public void JobEntity_RoundTripConversion_PreservesData()
    {
        // Arrange
        var originalModel = new Job
        {
            Id = Guid.NewGuid(),
            LinkedInJobId = "round-trip-test",
            Title = "Full Stack Developer",
            Company = "Big Tech",
            Location = "San Francisco, CA",
            IsRemote = true,
            IsContract = false,
            PayRate = "$180k",
            IsTopApplicant = true,
            IsPromoted = false,
            Description = "Build amazing products",
            Url = "https://linkedin.com/jobs/roundtrip",
            Score = 95,
            Status = JobStatus.Interviewing,
            CreatedAt = DateTime.UtcNow.AddDays(-7),
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var entity = JobEntity.FromModel(originalModel);
        var roundTrippedModel = entity.ToModel();

        // Assert
        roundTrippedModel.Id.Should().Be(originalModel.Id);
        roundTrippedModel.LinkedInJobId.Should().Be(originalModel.LinkedInJobId);
        roundTrippedModel.Title.Should().Be(originalModel.Title);
        roundTrippedModel.Company.Should().Be(originalModel.Company);
        roundTrippedModel.Location.Should().Be(originalModel.Location);
        roundTrippedModel.IsRemote.Should().Be(originalModel.IsRemote);
        roundTrippedModel.IsContract.Should().Be(originalModel.IsContract);
        roundTrippedModel.PayRate.Should().Be(originalModel.PayRate);
        roundTrippedModel.IsTopApplicant.Should().Be(originalModel.IsTopApplicant);
        roundTrippedModel.IsPromoted.Should().Be(originalModel.IsPromoted);
        roundTrippedModel.Description.Should().Be(originalModel.Description);
        roundTrippedModel.Url.Should().Be(originalModel.Url);
        roundTrippedModel.Score.Should().Be(originalModel.Score);
        roundTrippedModel.Status.Should().Be(originalModel.Status);
        roundTrippedModel.CreatedAt.Should().Be(originalModel.CreatedAt);
        roundTrippedModel.UpdatedAt.Should().Be(originalModel.UpdatedAt);
    }

    [Fact]
    public void JobEntity_DefaultValues_AreSet()
    {
        // Arrange & Act
        var entity = new JobEntity();

        // Assert
        entity.Id.Should().Be(Guid.Empty); // Default Guid
        entity.LinkedInJobId.Should().BeEmpty();
        entity.Title.Should().BeEmpty();
        entity.Company.Should().BeEmpty();
        entity.Url.Should().BeEmpty();
        entity.Status.Should().Be(JobStatus.Found); // Default enum value
        entity.Score.Should().Be(0);
    }
}
