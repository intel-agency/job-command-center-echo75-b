using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Custom Serilog enricher that adds job processing context and distributed tracing information.
/// Provides JobId, CorrelationId, and OperationId properties for log correlation and debugging.
/// Also enriches with HTTP request context when available.
/// </summary>
public class JobContextEnricher : ILogEventEnricher
{
    /// <summary>
    /// The property name for the job identifier in log events.
    /// </summary>
    public const string JobIdPropertyName = "JobId";

    /// <summary>
    /// The property name for the correlation identifier in log events.
    /// </summary>
    public const string CorrelationIdPropertyName = "CorrelationId";

    /// <summary>
    /// The property name for the operation identifier in log events.
    /// </summary>
    public const string OperationIdPropertyName = "OperationId";

    /// <summary>
    /// The property name for the trace identifier in log events.
    /// </summary>
    public const string TraceIdPropertyName = "TraceId";

    /// <summary>
    /// The property name for the span identifier in log events.
    /// </summary>
    public const string SpanIdPropertyName = "SpanId";

    /// <summary>
    /// The property name for the request identifier in log events.
    /// </summary>
    public const string RequestIdPropertyName = "RequestId";

    /// <summary>
    /// The property name for the request path in log events.
    /// </summary>
    public const string RequestPathPropertyName = "RequestPath";

    /// <summary>
    /// The property name for the HTTP method in log events.
    /// </summary>
    public const string HttpMethodPropertyName = "HttpMethod";

    /// <summary>
    /// Standard HTTP header names for correlation/request IDs.
    /// </summary>
    private static readonly string[] CorrelationHeaderNames =
    [
        "X-Correlation-ID",
        "X-CorrelationId",
        "X-Request-ID",
        "X-RequestId",
        "Request-Id",
        "Correlation-Id"
    ];

    // AsyncLocal storage for context propagation across async boundaries
    private static readonly AsyncLocal<string?> CurrentJobId = new();
    private static readonly AsyncLocal<string?> CurrentCorrelationId = new();
    private static readonly AsyncLocal<string?> CurrentOperationId = new();

    // Lazy-initialized IHttpContextAccessor for HTTP request enrichment
    private static IHttpContextAccessor? _httpContextAccessor;
    private static readonly object _lock = new();

    /// <summary>
    /// Gets or sets the HttpContextAccessor for HTTP request context enrichment.
    /// This should be set during service configuration.
    /// </summary>
    public static IHttpContextAccessor? HttpContextAccessor
    {
        get => _httpContextAccessor;
        set
        {
            lock (_lock)
            {
                _httpContextAccessor = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets the current job identifier for the async context.
    /// </summary>
    public static string? JobId
    {
        get => CurrentJobId.Value;
        set => CurrentJobId.Value = value;
    }

    /// <summary>
    /// Gets or sets the current correlation identifier for the async context.
    /// </summary>
    public static string? CorrelationId
    {
        get => CurrentCorrelationId.Value;
        set => CurrentCorrelationId.Value = value;
    }

    /// <summary>
    /// Gets or sets the current operation identifier for the async context.
    /// </summary>
    public static string? OperationId
    {
        get => CurrentOperationId.Value;
        set => CurrentOperationId.Value = value;
    }

    /// <summary>
    /// Enriches the log event with job context and distributed tracing information.
    /// </summary>
    /// <param name="logEvent">The log event to enrich.</param>
    /// <param name="propertyFactory">Factory for creating log event properties.</param>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        // Add job-specific context from AsyncLocal storage
        AddPropertyIfNotNull(logEvent, propertyFactory, JobIdPropertyName, JobId);
        AddPropertyIfNotNull(logEvent, propertyFactory, CorrelationIdPropertyName, CorrelationId);
        AddPropertyIfNotNull(logEvent, propertyFactory, OperationIdPropertyName, OperationId);

        // Add distributed tracing context from Activity (OpenTelemetry)
        EnrichFromActivity(logEvent, propertyFactory);

        // Add HTTP request context if available
        EnrichFromHttpContext(logEvent, propertyFactory);
    }

    /// <summary>
    /// Adds a property to the log event if the value is not null or empty.
    /// </summary>
    /// <param name="logEvent">The log event to enrich.</param>
    /// <param name="propertyFactory">Factory for creating log event properties.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="value">The value of the property.</param>
    private static void AddPropertyIfNotNull(
        LogEvent logEvent,
        ILogEventPropertyFactory propertyFactory,
        string propertyName,
        string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(propertyName, value));
        }
    }

    /// <summary>
    /// Enriches the log event with trace and span IDs from the current Activity.
    /// This integrates with OpenTelemetry for distributed tracing.
    /// </summary>
    /// <param name="logEvent">The log event to enrich.</param>
    /// <param name="propertyFactory">Factory for creating log event properties.</param>
    private static void EnrichFromActivity(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var activity = Activity.Current;
        if (activity is null)
        {
            return;
        }

        // Add TraceId for distributed trace correlation
        if (activity.TraceId != default)
        {
            var traceId = activity.TraceId.ToString();
            AddPropertyIfNotNull(logEvent, propertyFactory, TraceIdPropertyName, traceId);
        }

        // Add SpanId for identifying the current operation within a trace
        if (activity.SpanId != default)
        {
            var spanId = activity.SpanId.ToString();
            AddPropertyIfNotNull(logEvent, propertyFactory, SpanIdPropertyName, spanId);
        }

        // Add ParentSpanId for tracing the call chain
        if (activity.ParentSpanId != default)
        {
            var parentSpanId = activity.ParentSpanId.ToString();
            AddPropertyIfNotNull(logEvent, propertyFactory, "ParentSpanId", parentSpanId);
        }
    }

    /// <summary>
    /// Enriches the log event with HTTP request context from IHttpContextAccessor.
    /// Adds RequestId, RequestPath, and HttpMethod when available.
    /// </summary>
    /// <param name="logEvent">The log event to enrich.</param>
    /// <param name="propertyFactory">Factory for creating log event properties.</param>
    private static void EnrichFromHttpContext(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = _httpContextAccessor?.HttpContext;
        if (httpContext is null)
        {
            return;
        }

        var request = httpContext.Request;

        // Add request path for identifying the endpoint
        AddPropertyIfNotNull(logEvent, propertyFactory, RequestPathPropertyName, request.Path.Value);

        // Add HTTP method for request type identification
        AddPropertyIfNotNull(logEvent, propertyFactory, HttpMethodPropertyName, request.Method);

        // Try to get correlation ID from various headers
        var correlationId = GetCorrelationIdFromHeaders(request);
        if (!string.IsNullOrEmpty(correlationId))
        {
            // Set correlation ID if not already set via AsyncLocal
            if (string.IsNullOrEmpty(CorrelationId))
            {
                AddPropertyIfNotNull(logEvent, propertyFactory, CorrelationIdPropertyName, correlationId);
            }
        }

        // Add ASP.NET Core trace identifier as RequestId
        var traceIdentifier = httpContext.TraceIdentifier;
        if (!string.IsNullOrEmpty(traceIdentifier))
        {
            AddPropertyIfNotNull(logEvent, propertyFactory, RequestIdPropertyName, traceIdentifier);
        }
    }

    /// <summary>
    /// Gets the correlation ID from HTTP request headers.
    /// Checks multiple standard header names for flexibility.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <returns>The correlation ID if found, otherwise null.</returns>
    private static string? GetCorrelationIdFromHeaders(HttpRequest request)
    {
        foreach (var headerName in CorrelationHeaderNames)
        {
            if (request.Headers.TryGetValue(headerName, out var values) && values.Count > 0)
            {
                return values[0];
            }
        }

        return null;
    }

    /// <summary>
    /// Creates a scoped context for job processing that automatically cleans up when disposed.
    /// </summary>
    /// <param name="jobId">The job identifier to set in context.</param>
    /// <param name="correlationId">Optional correlation identifier. If not provided, a new GUID will be generated.</param>
    /// <returns>A disposable scope that restores previous context values when disposed.</returns>
    public static IDisposable BeginJobScope(string jobId, string? correlationId = null)
    {
        return new JobContextScope(jobId, correlationId ?? Guid.NewGuid().ToString());
    }

    /// <summary>
    /// Creates a scoped context for an operation with automatic cleanup.
    /// </summary>
    /// <param name="operationId">The operation identifier. If not provided, a new GUID will be generated.</param>
    /// <returns>A disposable scope that restores previous context values when disposed.</returns>
    public static IDisposable BeginOperationScope(string? operationId = null)
    {
        return new OperationContextScope(operationId ?? Guid.NewGuid().ToString());
    }

    /// <summary>
    /// Clears all context values. Use with caution - primarily for testing.
    /// </summary>
    public static void ClearContext()
    {
        JobId = null;
        CorrelationId = null;
        OperationId = null;
    }

    /// <summary>
    /// Disposable scope for job context that restores previous values on disposal.
    /// </summary>
    private sealed class JobContextScope : IDisposable
    {
        private readonly string? _previousJobId;
        private readonly string? _previousCorrelationId;
        private bool _disposed;

        public JobContextScope(string jobId, string correlationId)
        {
            _previousJobId = JobId;
            _previousCorrelationId = CorrelationId;
            JobId = jobId;
            CorrelationId = correlationId;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            JobId = _previousJobId;
            CorrelationId = _previousCorrelationId;
            _disposed = true;
        }
    }

    /// <summary>
    /// Disposable scope for operation context that restores previous values on disposal.
    /// </summary>
    private sealed class OperationContextScope : IDisposable
    {
        private readonly string? _previousOperationId;
        private bool _disposed;

        public OperationContextScope(string operationId)
        {
            _previousOperationId = OperationId;
            OperationId = operationId;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            OperationId = _previousOperationId;
            _disposed = true;
        }
    }
}
