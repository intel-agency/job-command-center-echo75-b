using JobCommandCenter.Data;
using JobCommandCenter.Data.Entities;
using JobCommandCenter.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace JobCommandCenter.IntegrationTests.Data;

/// <summary>
/// Integration tests for <see cref="AppDbContext"/> using InMemory database.
/// These tests verify the DbContext works correctly without requiring a real PostgreSQL instance.
/// </summary>
public class AppDbContextIntegrationTests : IAsyncLifetime
{
    private readonly AppDbContext _context;

    public AppDbContextIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
    }

    public async Task InitializeAsync()
    {
        // Ensure database is created
        await _context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task CanInsertAndRetrieveJob()
    {
        // Arrange
        var job = new JobEntity
        {
            Id = Guid.NewGuid(),
            LinkedInJobId = "integration-test-1",
            Title = "Integration Test Engineer",
            Company = "Test Corp",
            Url = "https://linkedin.com/jobs/integration-test-1",
            Status = JobStatus.Found,
            Score = 75,
            IsRemote = true,
            Description = "A comprehensive test job description"
        };

        // Act
        _context.Jobs.Add(job);
        await _context.SaveChangesAsync();

        var retrieved = await _context.Jobs
            .FirstOrDefaultAsync(j => j.LinkedInJobId == "integration-test-1");

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Title.Should().Be("Integration Test Engineer");
        retrieved.Company.Should().Be("Test Corp");
        retrieved.IsRemote.Should().BeTrue();
    }

    [Fact]
    public async Task CanQueryJobsByStatus()
    {
        // Arrange
        var jobs = new[]
        {
            new JobEntity { Id = Guid.NewGuid(), LinkedInJobId = "status-1", Title = "Job 1", Company = "C1", Url = "url1", Status = JobStatus.Found },
            new JobEntity { Id = Guid.NewGuid(), LinkedInJobId = "status-2", Title = "Job 2", Company = "C2", Url = "url2", Status = JobStatus.Applied },
            new JobEntity { Id = Guid.NewGuid(), LinkedInJobId = "status-3", Title = "Job 3", Company = "C3", Url = "url3", Status = JobStatus.Found },
            new JobEntity { Id = Guid.NewGuid(), LinkedInJobId = "status-4", Title = "Job 4", Company = "C4", Url = "url4", Status = JobStatus.Interviewing }
        };

        _context.Jobs.AddRange(jobs);
        await _context.SaveChangesAsync();

        // Act
        var foundJobs = await _context.Jobs
            .Where(j => j.Status == JobStatus.Found)
            .ToListAsync();

        // Assert
        foundJobs.Should().HaveCount(2);
        foundJobs.All(j => j.Status == JobStatus.Found).Should().BeTrue();
    }

    [Fact]
    public async Task CanQueryJobsByScore()
    {
        // Arrange
        var jobs = new[]
        {
            new JobEntity { Id = Guid.NewGuid(), LinkedInJobId = "score-1", Title = "Job 1", Company = "C1", Url = "url1", Score = 50 },
            new JobEntity { Id = Guid.NewGuid(), LinkedInJobId = "score-2", Title = "Job 2", Company = "C2", Url = "url2", Score = 80 },
            new JobEntity { Id = Guid.NewGuid(), LinkedInJobId = "score-3", Title = "Job 3", Company = "C3", Url = "url3", Score = 95 },
            new JobEntity { Id = Guid.NewGuid(), LinkedInJobId = "score-4", Title = "Job 4", Company = "C4", Url = "url4", Score = 30 }
        };

        _context.Jobs.AddRange(jobs);
        await _context.SaveChangesAsync();

        // Act
        var highScoringJobs = await _context.Jobs
            .Where(j => j.Score >= 80)
            .OrderByDescending(j => j.Score)
            .ToListAsync();

        // Assert
        highScoringJobs.Should().HaveCount(2);
        highScoringJobs[0].Score.Should().Be(95);
        highScoringJobs[1].Score.Should().Be(80);
    }

    [Fact]
    public async Task CanUpdateJobStatus()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var job = new JobEntity
        {
            Id = jobId,
            LinkedInJobId = "update-test",
            Title = "Update Test Job",
            Company = "Test Co",
            Url = "url",
            Status = JobStatus.Found
        };

        // Use a shared database name for this test
        var databaseName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: databaseName)
            .Options;

        // Add the job
        await using (var addContext = new AppDbContext(options))
        {
            addContext.Jobs.Add(job);
            await addContext.SaveChangesAsync();
        }

        // Act - Update the job
        await using (var updateContext = new AppDbContext(options))
        {
            var jobToUpdate = await updateContext.Jobs.FirstAsync(j => j.LinkedInJobId == "update-test");
            jobToUpdate.Status = JobStatus.Applied;
            updateContext.Jobs.Update(jobToUpdate);
            await updateContext.SaveChangesAsync();
        }

        // Assert
        await using (var verifyContext = new AppDbContext(options))
        {
            var verified = await verifyContext.Jobs.FirstAsync(j => j.LinkedInJobId == "update-test");
            verified.Status.Should().Be(JobStatus.Applied);
        }
    }

    [Fact]
    public async Task CanDeleteJob()
    {
        // Arrange
        var job = new JobEntity
        {
            Id = Guid.NewGuid(),
            LinkedInJobId = "delete-test",
            Title = "Delete Test Job",
            Company = "Test Co",
            Url = "url"
        };

        _context.Jobs.Add(job);
        await _context.SaveChangesAsync();

        // Act
        _context.Jobs.Remove(job);
        await _context.SaveChangesAsync();

        // Assert
        var deleted = await _context.Jobs.FindAsync(job.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task CanQueryJobsByRemoteStatus()
    {
        // Arrange
        var jobs = new[]
        {
            new JobEntity { Id = Guid.NewGuid(), LinkedInJobId = "remote-1", Title = "Remote Job 1", Company = "C1", Url = "url1", IsRemote = true },
            new JobEntity { Id = Guid.NewGuid(), LinkedInJobId = "remote-2", Title = "Office Job", Company = "C2", Url = "url2", IsRemote = false },
            new JobEntity { Id = Guid.NewGuid(), LinkedInJobId = "remote-3", Title = "Remote Job 2", Company = "C3", Url = "url3", IsRemote = true }
        };

        _context.Jobs.AddRange(jobs);
        await _context.SaveChangesAsync();

        // Act
        var remoteJobs = await _context.Jobs
            .Where(j => j.IsRemote)
            .ToListAsync();

        // Assert
        remoteJobs.Should().HaveCount(2);
        remoteJobs.All(j => j.IsRemote).Should().BeTrue();
    }
}
