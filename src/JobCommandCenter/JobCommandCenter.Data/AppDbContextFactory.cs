using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace JobCommandCenter.Data;

/// <summary>
/// Design-time factory for creating <see cref="AppDbContext"/> instances.
/// Used by EF Core CLI tools for migrations.
/// </summary>
/// <remarks>
/// This factory is only used at design time (when running migrations).
/// At runtime, the DbContext is configured via dependency injection.
/// </remarks>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    /// <summary>
    /// Creates a new instance of <see cref="AppDbContext"/> for design-time use.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>A configured <see cref="AppDbContext"/> instance.</returns>
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // Connection string must be provided via environment variable for design-time (migrations)
        // The actual connection string is provided at runtime via Aspire
        var connectionString = Environment.GetEnvironmentVariable("JOBDB_CONNECTION_STRING")
            ?? throw new InvalidOperationException(
                "The 'JOBDB_CONNECTION_STRING' environment variable is not set. " +
                "Please set this variable to run EF Core CLI commands. " +
                "Example: export JOBDB_CONNECTION_STRING='Host=localhost;Database=jobdb;Username=postgres;Password=yourpassword'");

        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
        });

        return new AppDbContext(optionsBuilder.Options);
    }
}
