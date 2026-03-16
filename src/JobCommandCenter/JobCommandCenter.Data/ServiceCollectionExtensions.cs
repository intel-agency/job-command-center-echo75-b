using JobCommandCenter.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JobCommandCenter.Data;

/// <summary>
/// Extension methods for configuring Job Command Center data services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Job Command Center data services to the dependency injection container.
    /// Configures EF Core DbContext with Npgsql provider, connection resiliency,
    /// and query tracking optimization.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="connectionString">The PostgreSQL connection string.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// <para>
    /// This method configures:
    /// <list type="bullet">
    ///   <item><description>DbContext pooling for improved performance</description></item>
    ///   <item><description>Npgsql provider with retry logic for transient failures</description></item>
    ///   <item><description>NoTracking query behavior for read-heavy scenarios</description></item>
    ///   <item><description>Migration assembly configuration</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var services = new ServiceCollection();
    /// services.AddJobCommandCenterData("Host=localhost;Database=jobdb;Username=postgres");
    /// </code>
    /// </example>
    public static IServiceCollection AddJobCommandCenterData(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrEmpty(connectionString);

        services.AddDbContextPool<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
                npgsqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
            });

            // Use NoTracking for read-heavy scenarios to improve performance
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

            // Enable sensitive data logging in development for debugging
#if DEBUG
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
#endif
        });

        return services;
    }

    /// <summary>
    /// Adds the Job Command Center data services using an existing DbContextOptions configuration.
    /// This overload is useful for Aspire integration where the connection is managed externally.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configureOptions">An action to configure the DbContextOptions.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddJobCommandCenterData(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.AddDbContextPool<AppDbContext>(options =>
        {
            configureOptions(options);

            // Ensure NoTracking is always applied for consistency
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

#if DEBUG
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
#endif
        });

        return services;
    }
}
