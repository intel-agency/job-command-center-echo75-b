using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ServiceDiscovery;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;

namespace Microsoft.Extensions.Hosting;

// Adds common Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
public static class Extensions
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";

    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        // Configure Serilog for structured logging
        builder.ConfigureSerilog();

        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();

            // Turn on service discovery by default
            http.AddServiceDiscovery();
        });

        // Uncomment the following to restrict the allowed schemes for service discovery.
        // builder.Services.Configure<ServiceDiscoveryOptions>(options =>
        // {
        //     options.AllowedSchemes = ["https"];
        // });

        return builder;
    }

    /// <summary>
    /// Configures Serilog as the logging provider with environment-aware settings.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the host application builder.</typeparam>
    /// <param name="builder">The host application builder.</param>
    /// <returns>The builder for chaining.</returns>
    public static TBuilder ConfigureSerilog<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddSerilog((services, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .ConfigureMinimumLevelFromConfiguration(builder.Configuration, builder.Environment)
                .ConfigureSinksFromConfiguration(builder.Configuration, builder.Environment);
        });

        return builder;
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource(builder.Environment.ApplicationName)
                    .AddAspNetCoreInstrumentation(tracing =>
                        // Exclude health check requests from tracing
                        tracing.Filter = context =>
                            !context.Request.Path.StartsWithSegments(HealthEndpointPath)
                            && !context.Request.Path.StartsWithSegments(AlivenessEndpointPath)
                    )
                    // Uncomment the following line to enable gRPC instrumentation (requires the OpenTelemetry.Instrumentation.GrpcNetClient package)
                    //.AddGrpcClientInstrumentation()
                    .AddHttpClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        // Uncomment the following lines to enable the Azure Monitor exporter (requires the Azure.Monitor.OpenTelemetry.AspNetCore package)
        //if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
        //{
        //    builder.Services.AddOpenTelemetry()
        //       .UseAzureMonitor();
        //}

        return builder;
    }

    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Adding health checks endpoints to applications in non-development environments has security implications.
        // See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
        if (app.Environment.IsDevelopment())
        {
            // All health checks must pass for app to be considered ready to accept traffic after starting
            app.MapHealthChecks(HealthEndpointPath);

            // Only health checks tagged with the "live" tag must pass for app to be considered alive
            app.MapHealthChecks(AlivenessEndpointPath, new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live")
            });
        }

        return app;
    }

    #region Serilog Configuration Helpers

    private const string DefaultSerilogOutputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";
    private const string DefaultLogFilePath = "logs/jcc-.log";
    private const int RetainedFileCountLimit = 31;
    private const long FileSizeLimitBytes = 10 * 1024 * 1024; // 10 MB

    /// <summary>
    /// Configures the minimum log level from configuration with environment-aware defaults.
    /// </summary>
    private static LoggerConfiguration ConfigureMinimumLevelFromConfiguration(
        this LoggerConfiguration loggerConfig,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // Try to get minimum level from configuration first
        var configuredLevel = configuration["Serilog:MinimumLevel:Default"];

        var minimumLevel = configuredLevel switch
        {
            "Verbose" => LogEventLevel.Verbose,
            "Debug" => LogEventLevel.Debug,
            "Information" => LogEventLevel.Information,
            "Warning" => LogEventLevel.Warning,
            "Error" => LogEventLevel.Error,
            "Fatal" => LogEventLevel.Fatal,
            _ => GetDefaultMinimumLevel(environment)
        };

        return loggerConfig
            .MinimumLevel.Is(minimumLevel)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("System", LogEventLevel.Warning);
    }

    /// <summary>
    /// Gets the default minimum log level based on the hosting environment.
    /// </summary>
    private static LogEventLevel GetDefaultMinimumLevel(IHostEnvironment environment)
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
    /// Configures Serilog sinks from configuration with sensible defaults.
    /// </summary>
    private static LoggerConfiguration ConfigureSinksFromConfiguration(
        this LoggerConfiguration loggerConfig,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // Console sink - always enabled
        var consoleMinimumLevel = environment.IsDevelopment()
            ? LogEventLevel.Debug
            : LogEventLevel.Information;

        loggerConfig.WriteTo.Console(
            outputTemplate: DefaultSerilogOutputTemplate,
            restrictedToMinimumLevel: consoleMinimumLevel);

        // File sink - with rolling files
        var logFilePath = configuration["Serilog:WriteTo:1:Args:path"]
            ?? configuration["Serilog:File:Path"]
            ?? DefaultLogFilePath;

        loggerConfig.WriteTo.File(
            path: logFilePath,
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: RetainedFileCountLimit,
            fileSizeLimitBytes: FileSizeLimitBytes,
            rollOnFileSizeLimit: true,
            outputTemplate: DefaultSerilogOutputTemplate);

        return loggerConfig;
    }

    #endregion
}
