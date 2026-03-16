using JobCommandCenter.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobCommandCenter.Data;

/// <summary>
/// Entity Framework Core database context for Job Command Center.
/// Provides access to job data persisted in PostgreSQL.
/// </summary>
/// <remarks>
/// <para>
/// This context is configured for:
/// <list type="bullet">
///   <item><description>Connection pooling via AddDbContextPool</description></item>
///   <item><description>NoTracking queries for read-heavy scenarios</description></item>
///   <item><description>Automatic retry on transient failures</description></item>
/// </list>
/// </para>
/// <para>
/// Use <see cref="ServiceCollectionExtensions.AddJobCommandCenterData"/> to register this context.
/// </para>
/// </remarks>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Gets the DbSet for jobs in the pipeline.
    /// </summary>
    public DbSet<JobEntity> Jobs => Set<JobEntity>();

    /// <summary>
    /// Creates a new instance of the database context.
    /// </summary>
    /// <param name="options">The options to configure the context.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Configures the model relationships and constraints.
    /// Applies all entity configurations from the assembly.
    /// </summary>
    /// <param name="modelBuilder">The builder to configure the model.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all IEntityTypeConfiguration implementations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    /// <summary>
    /// Saves changes to the database, automatically updating timestamps.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    /// <summary>
    /// Saves changes to the database asynchronously, automatically updating timestamps.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of state entries written to the database.</returns>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Updates the UpdatedAt timestamp for all modified entities.
    /// </summary>
    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<JobEntity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }

            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
    }
}
