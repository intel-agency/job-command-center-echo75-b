using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

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
    /// Default ANSI theme for console output with colored log levels.
    /// </summary>
    private static readonly AnsiConsoleTheme DefaultTheme = new AnsiConsoleTheme(
        new Dictionary<ConsoleThemeStyle, string>
        {
            [ConsoleThemeStyle.Text] = "\x1b[37m",                           // White (default text)
            [ConsoleThemeStyle.SecondaryText] = "\x1b[90m",                  // Bright black (gray)
            [ConsoleThemeStyle.TertiaryText] = "\x1b[90m",                   // Bright black (gray)
            [ConsoleThemeStyle.Invalid] = "\x1b[31;1m",                      // Red bold
            [ConsoleThemeStyle.Null] = "\x1b[95m",                           // Bright magenta
            [ConsoleThemeStyle.Name] = "\x1b[93m",                           // Bright yellow
            [ConsoleThemeStyle.String] = "\x1b[96m",                         // Bright cyan
            [ConsoleThemeStyle.Number] = "\x1b[95m",                         // Bright magenta
            [ConsoleThemeStyle.Boolean] = "\x1b[95m",                        // Bright magenta
            [ConsoleThemeStyle.Scalar] = "\x1b[95m",                         // Bright magenta
            [ConsoleThemeStyle.LevelVerbose] = "\x1b[90m",                   // Bright black (gray)
            [ConsoleThemeStyle.LevelDebug] = "\x1b[37m",                     // White
            [ConsoleThemeStyle.LevelInformation] = "\x1b[36m",               // Cyan
            [ConsoleThemeStyle.LevelWarning] = "\x1b[33;1m",                 // Bright yellow bold
            [ConsoleThemeStyle.LevelError] = "\x1b[31;1m",                   // Red bold
            [ConsoleThemeStyle.LevelFatal] = "\x1b[37;41;1m",                // White on red bold
        });

    /// <summary>
    /// Default console output template with ANSI colors and structured readability.
    /// </summary>
    private const string DefaultConsoleTemplate =
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}" +
        "    └─ {SourceContext}{NewLine}{Exception}";

    /// <summary>
    /// Default file output template with full timestamps and machine-readable format.
    /// </summary>
    private const string DefaultFileTemplate =
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";

    /// <summary>
    /// Default file path pattern for rolling logs.
    /// </summary>
    private const string DefaultLogPath = "logs/jcc-.log";

    /// <summary>
    /// Default number of days to retain log files.
    /// </summary>
    private const int DefaultRetainedFileCount = 7;

    /// <summary>
    /// Minimum allowed retention days.
    /// </summary>
    private const int MinRetentionDays = 1;

    /// <summary>
    /// Maximum allowed retention days.
    /// </summary>
    private const int MaxRetentionDays = 90;

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
            .WriteTo.Console(
                theme: DefaultTheme,
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
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
        // Configure console sink with ANSI theme
        ConfigureConsoleSink(loggerConfig, configuration, environment);

        // Configure file sink with rolling files
        ConfigureFileSink(loggerConfig, configuration);

        return loggerConfig;
    }

    /// <summary>
    /// Configures the console sink with ANSI theme for colored output.
    /// Console sink is always enabled for visibility during development and production.
    /// </summary>
    /// <param name="loggerConfig">The logger configuration builder.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="environment">The hosting environment.</param>
    private static void ConfigureConsoleSink(LoggerConfiguration loggerConfig, IConfiguration configuration, IHostEnvironment environment)
    {
        // Allow custom output template via configuration, otherwise use default
        var consoleTemplate = configuration["Serilog:WriteTo:Console:outputTemplate"]
            ?? configuration["Serilog:WriteTo:0:Args:outputTemplate"]
            ?? DefaultConsoleTemplate;

        // Determine theme: use configured theme name or default ANSI theme
        var theme = GetConsoleTheme(configuration);

        // Get minimum level for console based on environment
        var consoleMinimumLevel = environment.IsDevelopment() ? LogEventLevel.Debug : LogEventLevel.Information;

        loggerConfig.WriteTo.Console(
            theme: theme,
            outputTemplate: consoleTemplate,
            restrictedToMinimumLevel: consoleMinimumLevel);
    }

    /// <summary>
    /// Configures the file sink with daily rolling policy and retention.
    /// Can be disabled via Serilog:WriteTo:File:Enabled = false or for containerized environments.
    /// </summary>
    /// <param name="loggerConfig">The logger configuration builder.</param>
    /// <param name="configuration">The application configuration.</param>
    private static void ConfigureFileSink(LoggerConfiguration loggerConfig, IConfiguration configuration)
    {
        // Check if file sink is explicitly disabled
        var fileSinkEnabled = GetFileSinkEnabled(configuration);
        if (!fileSinkEnabled)
        {
            return;
        }

        // Get file path (default to logs/jcc-.log for daily rolling)
        var filePath = configuration["Serilog:WriteTo:File:path"]
            ?? configuration["Serilog:WriteTo:1:Args:path"]
            ?? DefaultLogPath;

        // Ensure logs directory exists
        EnsureLogsDirectoryExists(filePath);

        // Get rolling interval (default to daily)
        var rollingInterval = GetRollingInterval(
            configuration["Serilog:WriteTo:File:rollingInterval"]
            ?? configuration["Serilog:WriteTo:1:Args:rollingInterval"]);

        // Get retention policy with validation (default 7 days, min 1, max 90)
        var retainedFileCount = GetRetainedFileCount(configuration);

        // Get file output template
        var fileTemplate = configuration["Serilog:WriteTo:File:outputTemplate"]
            ?? configuration["Serilog:WriteTo:1:Args:outputTemplate"]
            ?? DefaultFileTemplate;

        // Configure shared flag for concurrent access
        var shared = configuration.GetValue<bool?>("Serilog:WriteTo:File:shared")
            ?? configuration.GetValue<bool?>("Serilog:WriteTo:1:Args:shared")
            ?? true;

        loggerConfig.WriteTo.File(
            path: filePath,
            outputTemplate: fileTemplate,
            rollingInterval: rollingInterval,
            retainedFileCountLimit: retainedFileCount,
            fileSizeLimitBytes: FileSizeLimitBytes,
            rollOnFileSizeLimit: true,
            shared: shared,
            flushToDiskInterval: TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Gets the console theme based on configuration.
    /// Supports "Ansi", "None", or defaults to custom ANSI theme.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The console theme to use.</returns>
    private static AnsiConsoleTheme? GetConsoleTheme(IConfiguration configuration)
    {
        var themeName = configuration["Serilog:WriteTo:Console:theme"]
            ?? configuration["Serilog:WriteTo:0:Args:theme"];

        return themeName?.ToLowerInvariant() switch
        {
            "none" or "null" => null,
            "code" => AnsiConsoleTheme.Code,
            _ => DefaultTheme
        };
    }

    /// <summary>
    /// Determines if file sink is enabled based on configuration and environment.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>True if file sink should be enabled, false otherwise.</returns>
    private static bool GetFileSinkEnabled(IConfiguration configuration)
    {
        // Check explicit enable/disable setting
        var enabled = configuration.GetValue<bool?>("Serilog:WriteTo:File:enabled")
            ?? configuration.GetValue<bool?>("Serilog:WriteTo:1:Args:enabled");

        if (enabled.HasValue)
        {
            return enabled.Value;
        }

        // Check for containerized environment indicator
        var isContainerized = configuration.GetValue<bool?>("DOTNET_RUNNING_IN_CONTAINER")
            ?? configuration.GetValue<bool?>("Containerized");

        // In containerized environments, file sink can be disabled by default
        // unless explicitly enabled
        if (isContainerized == true)
        {
            return configuration.GetValue<bool>("Serilog:WriteTo:File:enableInContainer", false);
        }

        // Default: file sink is enabled
        return true;
    }

    /// <summary>
    /// Gets the retained file count from configuration with validation.
    /// Default: 7 days, Minimum: 1 day, Maximum: 90 days.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The validated retention count.</returns>
    private static int GetRetainedFileCount(IConfiguration configuration)
    {
        var retainedDays = configuration.GetValue<int?>("Serilog:WriteTo:File:retainedFileCountLimit")
            ?? configuration.GetValue<int?>("Serilog:WriteTo:1:Args:retainedFileCountLimit")
            ?? DefaultRetainedFileCount;

        // Clamp to valid range
        return Math.Clamp(retainedDays, MinRetentionDays, MaxRetentionDays);
    }

    /// <summary>
    /// Gets the rolling interval from a string value.
    /// </summary>
    /// <param name="intervalString">The string representation of the rolling interval.</param>
    /// <returns>The RollingInterval value.</returns>
    private static RollingInterval GetRollingInterval(string? intervalString)
    {
        return intervalString?.ToLowerInvariant() switch
        {
            "minute" => RollingInterval.Minute,
            "hour" => RollingInterval.Hour,
            "month" => RollingInterval.Month,
            "year" => RollingInterval.Year,
            _ => RollingInterval.Day
        };
    }

    /// <summary>
    /// Ensures the logs directory exists for the given file path.
    /// </summary>
    /// <param name="filePath">The log file path (may contain rolling pattern).</param>
    private static void EnsureLogsDirectoryExists(string filePath)
    {
        try
        {
            // Extract directory from path (handle rolling patterns like logs/jcc-.log)
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            // Log directory creation failed - this will be caught by Serilog later
            // Don't throw here as it would prevent the entire application from starting
        }
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
