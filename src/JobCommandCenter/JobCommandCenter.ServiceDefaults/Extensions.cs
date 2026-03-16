using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ServiceDiscovery;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
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
        // Register IHttpContextAccessor for request context enrichment
        builder.Services.AddHttpContextAccessor();

        // Configure Serilog from configuration with environment context
        var logger = SerilogConfiguration.ConfigureSerilog(
            builder.Configuration,
            builder.Environment.ApplicationName,
            builder.Environment.EnvironmentName);

        // Set Serilog as the logging provider
        builder.Services.AddSerilog(logger, dispose: true);

        // Configure HttpContextAccessor for the JobContextEnricher after services are built
        builder.Services.AddHostedService<LoggingContextInitializer>();

        return builder;
    }

    /// <summary>
    /// Hosted service that initializes the JobContextEnricher with IHttpContextAccessor
    /// after the service provider is built.
    /// </summary>
    private sealed class LoggingContextInitializer : IHostedService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LoggingContextInitializer(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Wire up the HttpContextAccessor to the enricher
            JobContextEnricher.HttpContextAccessor = _httpContextAccessor;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        // Build resource attributes for service identity
        var (serviceName, serviceVersion, serviceInstanceId, deploymentEnvironment) = GetServiceIdentity(builder.Configuration, builder.Environment);

        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
            logging.SetResourceBuilder(ResourceBuilder.CreateEmpty()
                .AddService(serviceName: serviceName, serviceVersion: serviceVersion, serviceInstanceId: serviceInstanceId)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = deploymentEnvironment,
                    ["host.name"] = Environment.MachineName,
                    ["process.pid"] = Environment.ProcessId,
                }));
        });

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                resource.AddService(
                    serviceName: serviceName,
                    serviceVersion: serviceVersion,
                    serviceInstanceId: serviceInstanceId);
                resource.AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = deploymentEnvironment,
                    ["host.name"] = Environment.MachineName,
                    ["process.pid"] = Environment.ProcessId,
                });

                // Add custom attributes from configuration
                AddCustomAttributesFromConfiguration(resource, builder.Configuration);
            })
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

                // Configure sampling for high-volume scenarios
                ConfigureSampling(tracing, builder.Configuration, builder.Environment);
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    /// <summary>
    /// Gets service identity information from configuration.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="environment">The hosting environment.</param>
    /// <returns>A tuple containing service name, version, instance ID, and deployment environment.</returns>
    private static (string ServiceName, string ServiceVersion, string ServiceInstanceId, string DeploymentEnvironment) 
        GetServiceIdentity(IConfiguration configuration, IHostEnvironment environment)
    {
        var serviceName = configuration["OTEL_SERVICE_NAME"]
            ?? configuration["Service:Name"]
            ?? environment.ApplicationName
            ?? "Unknown";

        var serviceVersion = configuration["OTEL_SERVICE_VERSION"]
            ?? configuration["Service:Version"]
            ?? GetAssemblyVersion();

        var deploymentEnvironment = configuration["OTEL_DEPLOYMENT_ENVIRONMENT"]
            ?? configuration["Deployment:Environment"]
            ?? environment.EnvironmentName
            ?? "Unknown";

        var serviceInstanceId = configuration["OTEL_SERVICE_INSTANCE_ID"]
            ?? configuration["Service:InstanceId"]
            ?? $"{Environment.MachineName}:{Environment.ProcessId}:{Guid.NewGuid():N}";

        return (serviceName, serviceVersion, serviceInstanceId, deploymentEnvironment);
    }

    /// <summary>
    /// Gets the assembly version for service identity.
    /// </summary>
    /// <returns>The assembly version string or "Unknown" if not available.</returns>
    private static string GetAssemblyVersion()
    {
        try
        {
            var assembly = System.Reflection.Assembly.GetEntryAssembly() ?? System.Reflection.Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            return version?.ToString() ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    /// <summary>
    /// Adds custom resource attributes from configuration.
    /// Looks for attributes under the "OTEL_RESOURCE_ATTRIBUTES" environment variable
    /// or the "OpenTelemetry:ResourceAttributes" configuration section.
    /// </summary>
    /// <param name="resourceBuilder">The resource builder to add attributes to.</param>
    /// <param name="configuration">The application configuration.</param>
    private static void AddCustomAttributesFromConfiguration(ResourceBuilder resourceBuilder, IConfiguration configuration)
    {
        // Parse OTEL_RESOURCE_ATTRIBUTES environment variable (format: key1=value1,key2=value2)
        var resourceAttributesEnv = configuration["OTEL_RESOURCE_ATTRIBUTES"];
        if (!string.IsNullOrEmpty(resourceAttributesEnv))
        {
            foreach (var pair in resourceAttributesEnv.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var keyValue = pair.Split('=', 2, StringSplitOptions.TrimEntries);
                if (keyValue.Length == 2)
                {
                    resourceBuilder.AddAttributes(new Dictionary<string, object>
                    {
                        [keyValue[0]] = keyValue[1]
                    });
                }
            }
        }

        // Also check configuration section for additional attributes
        var attributesSection = configuration.GetSection("OpenTelemetry:ResourceAttributes");
        if (attributesSection.Exists())
        {
            foreach (var attr in attributesSection.GetChildren())
            {
                if (!string.IsNullOrEmpty(attr.Value))
                {
                    resourceBuilder.AddAttributes(new Dictionary<string, object>
                    {
                        [attr.Key] = attr.Value
                    });
                }
            }
        }
    }

    /// <summary>
    /// Configures trace sampling for high-volume scenarios.
    /// Uses ParentBasedSampler with TraceIdRatioBasedSampler for production,
    /// and AlwaysOnSampler for development.
    /// </summary>
    /// <param name="tracing">The tracing provider builder.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="environment">The hosting environment.</param>
    private static void ConfigureSampling(
        TracerProviderBuilder tracing,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // Get sampling rate from configuration
        // Default: 1.0 (100%) for development, 0.1 (10%) for production
        var defaultRate = environment.IsDevelopment() ? 1.0 : 0.1;
        var samplingRate = configuration.GetValue<double?>("OpenTelemetry:Sampling:Rate")
            ?? configuration.GetValue<double?>("OTEL_TRACES_SAMPLER_ARG")
            ?? defaultRate;

        // Clamp to valid range
        samplingRate = Math.Clamp(samplingRate, 0.0, 1.0);

        // Check for explicit sampler type configuration
        var samplerType = configuration["OpenTelemetry:Sampling:Type"]
            ?? configuration["OTEL_TRACES_SAMPLER"]
            ?? "parentbased";

        Sampler sampler = samplerType.ToLowerInvariant() switch
        {
            "always_on" => new AlwaysOnSampler(),
            "always_off" => new AlwaysOffSampler(),
            "traceidratio" => new TraceIdRatioBasedSampler(samplingRate),
            "parentbased_always_on" => new ParentBasedSampler(new AlwaysOnSampler()),
            "parentbased_always_off" => new ParentBasedSampler(new AlwaysOffSampler()),
            "parentbased_traceidratio" => new ParentBasedSampler(new TraceIdRatioBasedSampler(samplingRate)),
            _ => new ParentBasedSampler(new TraceIdRatioBasedSampler(samplingRate)) // Default: parentbased
        };

        tracing.SetSampler(sampler);
    }

    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]!;
            var otlpProtocol = GetOtlpProtocol(builder.Configuration);
            var otlpHeaders = GetOtlpHeaders(builder.Configuration);
            var otlpTimeout = GetOtlpTimeout(builder.Configuration);

            builder.Services.Configure<OtlpExporterOptions>(options =>
            {
                options.Endpoint = new Uri(otlpEndpoint);
                options.Protocol = otlpProtocol;

                // Configure headers from environment/configuration
                if (otlpHeaders.Count > 0)
                {
                    options.Headers = string.Join(",", otlpHeaders.Select(h => $"{h.Key}={h.Value}"));
                }
            });

            builder.Services.Configure<BatchExportProcessorOptions<System.Diagnostics.Activity>>(options =>
            {
                options.ExporterTimeoutMilliseconds = otlpTimeout;
                options.MaxExportBatchSize = 512;
                options.ScheduledDelayMilliseconds = 5000;
                options.MaxQueueSize = 2048;
            });

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

    /// <summary>
    /// Gets the OTLP protocol from configuration.
    /// Supports "grpc" (default) and "http/protobuf".
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The configured OtlpExportProtocol.</returns>
    private static OtlpExportProtocol GetOtlpProtocol(IConfiguration configuration)
    {
        var protocolString = configuration["OTEL_EXPORTER_OTLP_PROTOCOL"]
            ?? configuration["OpenTelemetry:Otlp:Protocol"]
            ?? "grpc";

        return protocolString.ToLowerInvariant() switch
        {
            "http/protobuf" or "http" => OtlpExportProtocol.HttpProtobuf,
            _ => OtlpExportProtocol.Grpc
        };
    }

    /// <summary>
    /// Gets OTLP headers from configuration.
    /// Parses "OTEL_EXPORTER_OTLP_HEADERS" (format: key1=value1,key2=value2)
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>A dictionary of headers.</returns>
    private static Dictionary<string, string> GetOtlpHeaders(IConfiguration configuration)
    {
        var headers = new Dictionary<string, string>();

        var headersEnv = configuration["OTEL_EXPORTER_OTLP_HEADERS"]
            ?? configuration["OpenTelemetry:Otlp:Headers"];

        if (!string.IsNullOrEmpty(headersEnv))
        {
            foreach (var pair in headersEnv.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var keyValue = pair.Split('=', 2, StringSplitOptions.TrimEntries);
                if (keyValue.Length == 2)
                {
                    headers[keyValue[0]] = keyValue[1];
                }
            }
        }

        return headers;
    }

    /// <summary>
    /// Gets the OTLP export timeout from configuration.
    /// Default: 10000ms (10 seconds).
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The timeout in milliseconds.</returns>
    private static int GetOtlpTimeout(IConfiguration configuration)
    {
        return configuration.GetValue<int?>("OTEL_EXPORTER_OTLP_TIMEOUT")
            ?? configuration.GetValue<int?>("OpenTelemetry:Otlp:TimeoutMilliseconds")
            ?? 10000;
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
}
