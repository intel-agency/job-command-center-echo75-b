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

        // Use a default connection string for design-time (migrations only)
        // The actual connection string is provided at runtime via Aspire
        var connectionString = Environment.GetEnvironmentVariable("JOBDB_CONNECTION_STRING")
            ?? "Host=localhost;Database=jobdb;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
        });

        return new AppDbContext(optionsBuilder.Options);
    }
}
