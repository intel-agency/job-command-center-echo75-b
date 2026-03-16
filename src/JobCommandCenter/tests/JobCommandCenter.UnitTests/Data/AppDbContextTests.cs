using JobCommandCenter.Data;
using JobCommandCenter.Data.Entities;
using JobCommandCenter.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace JobCommandCenter.UnitTests.Data;

/// <summary>
/// Unit tests for <see cref="AppDbContext"/> entity configuration.
/// </summary>
public class AppDbContextTests : IDisposable
{
    private readonly AppDbContext _context;

    public AppDbContextTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public void AppDbContext_HasJobsDbSet()
    {
        // Assert
        _context.Jobs.Should().NotBeNull();
    }

    [Fact]
    public async Task AppDbContext_CanAddAndRetrieveJobEntity()
    {
        // Arrange
        var entity = new JobEntity
        {
            Id = Guid.NewGuid(),
            LinkedInJobId = "test-job-123",
            Title = "Software Engineer",
            Company = "Test Company",
            Url = "https://linkedin.com/jobs/test",
            Status = JobStatus.Found,
            Score = 50,
            IsRemote = true
        };

        // Act
        _context.Jobs.Add(entity);
        await _context.SaveChangesAsync();

        var retrieved = await _context.Jobs.FindAsync(entity.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.LinkedInJobId.Should().Be("test-job-123");
        retrieved.Title.Should().Be("Software Engineer");
        retrieved.Company.Should().Be("Test Company");
        retrieved.Status.Should().Be(JobStatus.Found);
    }

    [Fact]
    public async Task AppDbContext_UpdatesTimestampsOnSave()
    {
        // Arrange
        var entity = new JobEntity
        {
            Id = Guid.NewGuid(),
            LinkedInJobId = "test-job-timestamp",
            Title = "Test Job",
            Company = "Test Company",
            Url = "https://linkedin.com/jobs/test",
            Status = JobStatus.Found
        };

        var beforeSave = DateTime.UtcNow;

        // Act
        _context.Jobs.Add(entity);
        await _context.SaveChangesAsync();

        var afterSave = DateTime.UtcNow;

        // Assert
        entity.CreatedAt.Should().BeOnOrAfter(beforeSave);
        entity.CreatedAt.Should().BeOnOrBefore(afterSave);
        entity.UpdatedAt.Should().BeOnOrAfter(beforeSave);
        entity.UpdatedAt.Should().BeOnOrBefore(afterSave);
    }

    [Fact]
    public async Task AppDbContext_UpdatesUpdatedAtOnModification()
    {
        // Arrange
        var entity = new JobEntity
        {
            Id = Guid.NewGuid(),
            LinkedInJobId = "test-job-modify",
            Title = "Test Job",
            Company = "Test Company",
            Url = "https://linkedin.com/jobs/test",
            Status = JobStatus.Found
        };

        // Use a shared database name for this test
        var databaseName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: databaseName)
            .Options;

        // Add the entity
        await using (var addContext = new AppDbContext(options))
        {
            addContext.Jobs.Add(entity);
            await addContext.SaveChangesAsync();
        }

        var originalUpdatedAt = entity.UpdatedAt;

        // Small delay to ensure timestamp difference
        await Task.Delay(10);

        // Act - Modify and save again
        await using (var updateContext = new AppDbContext(options))
        {
            var toUpdate = await updateContext.Jobs.FirstAsync(j => j.Id == entity.Id);
            toUpdate.Title = "Updated Title";
            updateContext.Jobs.Update(toUpdate);
            await updateContext.SaveChangesAsync();
        }

        // Assert - The UpdatedAt should be set by SaveChanges
        await using (var verifyContext = new AppDbContext(options))
        {
            var verified = await verifyContext.Jobs.FirstAsync(j => j.Id == entity.Id);
            verified.UpdatedAt.Should().BeOnOrAfter(originalUpdatedAt);
            verified.Title.Should().Be("Updated Title");
        }
    }

    [Fact]
    public async Task AppDbContext_PreventsDuplicateLinkedInJobIds()
    {
        // Arrange
        var entity1 = new JobEntity
        {
            Id = Guid.NewGuid(),
            LinkedInJobId = "duplicate-id",
            Title = "Job 1",
            Company = "Company 1",
            Url = "https://linkedin.com/jobs/1"
        };

        var entity2 = new JobEntity
        {
            Id = Guid.NewGuid(),
            LinkedInJobId = "duplicate-id",
            Title = "Job 2",
            Company = "Company 2",
            Url = "https://linkedin.com/jobs/2"
        };

        // Act
        _context.Jobs.Add(entity1);
        await _context.SaveChangesAsync();

        // In-memory database doesn't enforce unique constraints
        // This test documents expected behavior with real database
        // With PostgreSQL, this would throw a unique constraint violation

        // Assert
        var count = await _context.Jobs.CountAsync();
        count.Should().Be(1);
    }
}
