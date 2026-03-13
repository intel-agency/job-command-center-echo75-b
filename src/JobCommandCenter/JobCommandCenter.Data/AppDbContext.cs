using JobCommandCenter.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobCommandCenter.Data;

/// <summary>
/// Entity Framework Core database context for Job Command Center.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Jobs in the pipeline.
    /// </summary>
    public DbSet<JobEntity> Jobs => Set<JobEntity>();

    /// <summary>
    /// Creates a new instance of the database context.
    /// </summary>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Configures the model relationships and constraints.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Job entity configuration
        modelBuilder.Entity<JobEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.LinkedInJobId).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Score);
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}
