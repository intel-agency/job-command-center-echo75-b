using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Provides Serilog configuration helpers for bootstrap logging and environment-aware setup.
/// </summary>
public static class SerilogConfiguration
{
    private const string DefaultOutputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";
    private const string DefaultLogFilePath = "logs/jcc-.log";
    private const string DefaultLogFileName = "jcc-.log";
    private const int RetainedFileCount = 31;
    private const long FileSizeLimitBytes = 10 * 1024 * 1024; // 10 MB

    /// <summary>
    /// Creates a bootstrap logger for capturing early startup errors before configuration is fully loaded.
    /// This logger is intended to be replaced by the full configured logger once the host is built.
    /// </summary>
    /// <returns>A bootstrap logger instance.</returns>
    public static ILogger CreateBootstrapLogger()
    {
        return new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: DefaultOutputTemplate)
            .CreateBootstrapLogger();
    }

    /// <summary>
    /// Creates a Serilog logger configuration based on the provided host builder context.
    /// </summary>
    /// <param name="context">The host builder context containing configuration and environment information.</param>
    /// <returns>A configured LoggerConfiguration ready for building.</returns>
    public static LoggerConfiguration CreateLoggerConfiguration(HostBuilderContext context)
    {
        var configuration = context.Configuration;
        var environment = context.HostingEnvironment;

        var loggerConfig = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .ConfigureMinimumLevel(environment)
            .ConfigureSinks(configuration, environment);

        return loggerConfig;
    }

    /// <summary>
    /// Configures the minimum log level based on the hosting environment.
    /// </summary>
    /// <param name="loggerConfig">The logger configuration to modify.</param>
    /// <param name="environment">The hosting environment.</param>
    /// <returns>The modified logger configuration for chaining.</returns>
    private static LoggerConfiguration ConfigureMinimumLevel(this LoggerConfiguration loggerConfig, IHostEnvironment environment)
    {
        var minimumLevel = GetMinimumLogLevel(environment);

        return loggerConfig
            .MinimumLevel.Is(minimumLevel)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("System", LogEventLevel.Warning);
    }

    /// <summary>
    /// Gets the minimum log level for the specified environment.
    /// </summary>
    /// <param name="environment">The hosting environment.</param>
    /// <returns>The appropriate minimum log level for the environment.</returns>
    public static LogEventLevel GetMinimumLogLevel(IHostEnvironment environment)
    {
        return environment.EnvironmentName switch
        {
            "Development" => LogEventLevel.Debug,
            "Staging" => LogEventLevel.Information,
            "Production" => LogEventLevel.Information,
            _ => LogEventLevel.Information
        };
    }

    /// <summary>
    /// Configures the output sinks based on configuration and environment.
    /// </summary>
    /// <param name="loggerConfig">The logger configuration to modify.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="environment">The hosting environment.</param>
    /// <returns>The modified logger configuration for chaining.</returns>
    private static LoggerConfiguration ConfigureSinks(
        this LoggerConfiguration loggerConfig,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // Always add console sink
        loggerConfig.WriteTo.Console(
            outputTemplate: DefaultOutputTemplate,
            restrictedToMinimumLevel: GetConsoleMinimumLevel(environment));

        // Add file sink with rolling files
        var logFilePath = configuration["Serilog:WriteTo:File:Path"] ?? DefaultLogFilePath;
        loggerConfig.WriteTo.File(
            path: logFilePath,
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: RetainedFileCount,
            fileSizeLimitBytes: FileSizeLimitBytes,
            rollOnFileSizeLimit: true,
            outputTemplate: DefaultOutputTemplate);

        return loggerConfig;
    }

    /// <summary>
    /// Gets the minimum log level for console output based on environment.
    /// </summary>
    /// <param name="environment">The hosting environment.</param>
    /// <returns>The minimum log level for console output.</returns>
    private static LogEventLevel GetConsoleMinimumLevel(IHostEnvironment environment)
    {
        return environment.IsDevelopment() ? LogEventLevel.Debug : LogEventLevel.Information;
    }
}
