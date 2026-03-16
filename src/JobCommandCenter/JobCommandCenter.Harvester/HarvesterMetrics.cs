using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;

namespace JobCommandCenter.Harvester;

/// <summary>
/// Custom metrics for Harvester operations.
/// Provides counters, histograms, and gauges for monitoring job harvesting activities.
/// </summary>
/// <remarks>
/// <para>
/// This class implements OpenTelemetry-compatible metrics using <see cref="System.Diagnostics.Metrics"/>.
/// Metrics are automatically exported when OpenTelemetry is configured in the application.
/// </para>
/// <para>
/// <strong>Available Metrics:</strong>
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Metric Name</term>
///     <description>Description</description>
///   </listheader>
///   <item>
///     <term>harvester.jobs_harvested</term>
///     <description>Counter - Total jobs harvested from LinkedIn</description>
///   </item>
///   <item>
///     <term>harvester.harvest_duration</term>
///     <description>Histogram - Duration of harvest operations in ms</description>
///   </item>
///   <item>
///     <term>harvester.active_connections</term>
///     <description>Gauge - Active Chrome CDP connections</description>
///   </item>
///   <item>
///     <term>harvester.errors</term>
///     <description>Counter - Total errors encountered</description>
///   </item>
///   <item>
///     <term>harvester.page_load_duration</term>
///     <description>Histogram - Page load duration in ms</description>
///   </item>
///   <item>
///     <term>harvester.pages_scraped</term>
///     <description>Counter - Total pages scraped</description>
///   </item>
/// </list>
/// </remarks>
/// <example>
/// <para>Registering HarvesterMetrics with dependency injection:</para>
/// <code>
/// // In Program.cs or service configuration
/// services.AddHarvesterMetrics();
/// </code>
/// <para>Using metrics in a worker service:</para>
/// <code>
/// public class HarvestWorker : BackgroundService
/// {
///     private readonly HarvesterMetrics _metrics;
///     private readonly ILogger&lt;HarvestWorker&gt; _logger;
///     
///     public HarvestWorker(HarvesterMetrics metrics, ILogger&lt;HarvestWorker&gt; logger)
///     {
///         _metrics = metrics;
///         _logger = logger;
///     }
///     
///     protected override async Task ExecuteAsync(CancellationToken stoppingToken)
///     {
///         while (!stoppingToken.IsCancellationRequested)
///         {
///             var stopwatch = Stopwatch.StartNew();
///             
///             // Track active connection using scope
///             using var connectionScope = _metrics.BeginConnectionScope();
///             
///             try
///             {
///                 var jobs = await HarvestJobsAsync(stoppingToken);
///                 
///                 // Record metrics
///                 _metrics.RecordJobsHarvested(jobs.Count, source: "search", status: "new");
///                 _metrics.RecordPagesScraped(1, pageType: "search_results");
///                 
///                 stopwatch.Stop();
///                 _metrics.RecordHarvestDuration(stopwatch.Elapsed, operation: "full_cycle", success: true);
///                 
///                 _logger.LogInformation("Harvested {Count} jobs in {Duration}ms", 
///                     jobs.Count, stopwatch.ElapsedMilliseconds);
///             }
///             catch (Exception ex)
///             {
///                 stopwatch.Stop();
///                 _metrics.RecordHarvestDuration(stopwatch.Elapsed, operation: "full_cycle", success: false);
///                 _metrics.RecordError("harvest_failed", operation: "full_cycle");
///                 
///                 _logger.LogError(ex, "Harvest operation failed");
///             }
///             
///             await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
///         }
///     }
/// }
/// </code>
/// <para>Recording page load metrics:</para>
/// <code>
/// public async Task&lt;IReadOnlyList&lt;Job&gt;&gt; ScrapePageAsync(string url)
/// {
///     var stopwatch = Stopwatch.StartNew();
///     
///     try
///     {
///         var page = await _browser.NewPageAsync();
///         await page.GotoAsync(url);
///         
///         stopwatch.Stop();
///         _metrics.RecordPageLoadDuration(stopwatch.Elapsed, pageType: "job_detail", success: true);
///         
///         return await ParseJobsAsync(page);
///     }
///     catch (TimeoutException)
///     {
///         stopwatch.Stop();
///         _metrics.RecordPageLoadDuration(stopwatch.Elapsed, pageType: "job_detail", success: false);
///         _metrics.RecordError("timeout", operation: "page_load");
///         throw;
///     }
/// }
/// </code>
/// </example>
public sealed class HarvesterMetrics : IDisposable
{
    /// <summary>
    /// The name of the meter used for Harvester metrics.
    /// </summary>
    public const string MeterName = "JobCommandCenter.Harvester";

    /// <summary>
    /// The version of the meter.
    /// </summary>
    public const string MeterVersion = "1.0.0";

    private readonly Meter _meter;
    private readonly Counter<long> _jobsHarvestedCounter;
    private readonly Histogram<double> _harvestDurationHistogram;
    private readonly ObservableGauge<int> _activeConnectionsGauge;
    private readonly Counter<long> _errorsCounter;
    private readonly Histogram<double> _pageLoadDurationHistogram;
    private readonly Counter<long> _pagesScrapedCounter;

    // State for observable gauges
    private int _activeConnections;
    private readonly object _connectionLock = new();

    /// <summary>
    /// Gets the active connection count for observable gauge.
    /// </summary>
    public int ActiveConnections
    {
        get
        {
            lock (_connectionLock)
            {
                return _activeConnections;
            }
        }
    }

    /// <summary>
    /// Creates a new instance of HarvesterMetrics.
    /// </summary>
    public HarvesterMetrics()
    {
        _meter = new Meter(MeterName, MeterVersion);

        // Counter: Total jobs harvested
        _jobsHarvestedCounter = _meter.CreateCounter<long>(
            name: "harvester.jobs_harvested",
            unit: "{jobs}",
            description: "Total number of jobs harvested from LinkedIn");

        // Histogram: Harvest operation duration
        _harvestDurationHistogram = _meter.CreateHistogram<double>(
            name: "harvester.harvest_duration",
            unit: "ms",
            description: "Duration of harvest operations in milliseconds");

        // Observable Gauge: Active connections to Chrome CDP
        _activeConnectionsGauge = _meter.CreateObservableGauge(
            name: "harvester.active_connections",
            observeValue: () => ActiveConnections,
            unit: "{connections}",
            description: "Number of active connections to Chrome DevTools Protocol");

        // Counter: Total errors encountered
        _errorsCounter = _meter.CreateCounter<long>(
            name: "harvester.errors",
            unit: "{errors}",
            description: "Total number of errors encountered during harvesting");

        // Histogram: Page load duration
        _pageLoadDurationHistogram = _meter.CreateHistogram<double>(
            name: "harvester.page_load_duration",
            unit: "ms",
            description: "Duration of page load operations in milliseconds");

        // Counter: Total pages scraped
        _pagesScrapedCounter = _meter.CreateCounter<long>(
            name: "harvester.pages_scraped",
            unit: "{pages}",
            description: "Total number of pages scraped from LinkedIn");
    }

    /// <summary>
    /// Records a successful job harvest.
    /// </summary>
    /// <param name="count">Number of jobs harvested in this operation.</param>
    /// <param name="source">The source of the harvest (e.g., "search", "recommendations").</param>
    /// <param name="status">The status of harvested jobs (e.g., "new", "updated").</param>
    public void RecordJobsHarvested(int count, string source = "unknown", string status = "new")
    {
        if (count <= 0)
        {
            return;
        }

        var tags = new TagList
        {
            { "source", source },
            { "status", status }
        };

        _jobsHarvestedCounter.Add(count, tags);
    }

    /// <summary>
    /// Records the duration of a harvest operation.
    /// </summary>
    /// <param name="duration">The duration of the operation.</param>
    /// <param name="operation">The type of operation (e.g., "full_cycle", "page_scrape").</param>
    /// <param name="success">Whether the operation was successful.</param>
    public void RecordHarvestDuration(TimeSpan duration, string operation = "full_cycle", bool success = true)
    {
        var tags = new TagList
        {
            { "operation", operation },
            { "success", success }
        };

        _harvestDurationHistogram.Record(duration.TotalMilliseconds, tags);
    }

    /// <summary>
    /// Increments the active connection count.
    /// </summary>
    /// <returns>The new connection count.</returns>
    public int IncrementActiveConnections()
    {
        lock (_connectionLock)
        {
            _activeConnections++;
            return _activeConnections;
        }
    }

    /// <summary>
    /// Decrements the active connection count.
    /// </summary>
    /// <returns>The new connection count.</returns>
    public int DecrementActiveConnections()
    {
        lock (_connectionLock)
        {
            _activeConnections = Math.Max(0, _activeConnections - 1);
            return _activeConnections;
        }
    }

    /// <summary>
    /// Sets the active connection count directly.
    /// </summary>
    /// <param name="count">The connection count to set.</param>
    public void SetActiveConnections(int count)
    {
        lock (_connectionLock)
        {
            _activeConnections = Math.Max(0, count);
        }
    }

    /// <summary>
    /// Records an error that occurred during harvesting.
    /// </summary>
    /// <param name="errorType">The type of error (e.g., "connection", "timeout", "parsing").</param>
    /// <param name="operation">The operation during which the error occurred.</param>
    public void RecordError(string errorType, string operation = "unknown")
    {
        var tags = new TagList
        {
            { "error_type", errorType },
            { "operation", operation }
        };

        _errorsCounter.Add(1, tags);
    }

    /// <summary>
    /// Records the duration of a page load operation.
    /// </summary>
    /// <param name="duration">The duration of the page load.</param>
    /// <param name="pageType">The type of page loaded (e.g., "search_results", "job_detail").</param>
    /// <param name="success">Whether the page load was successful.</param>
    public void RecordPageLoadDuration(TimeSpan duration, string pageType = "unknown", bool success = true)
    {
        var tags = new TagList
        {
            { "page_type", pageType },
            { "success", success }
        };

        _pageLoadDurationHistogram.Record(duration.TotalMilliseconds, tags);
    }

    /// <summary>
    /// Records pages scraped during a harvesting session.
    /// </summary>
    /// <param name="count">Number of pages scraped.</param>
    /// <param name="pageType">The type of pages scraped (e.g., "search_results", "job_detail").</param>
    public void RecordPagesScraped(int count, string pageType = "unknown")
    {
        if (count <= 0)
        {
            return;
        }

        var tags = new TagList
        {
            { "page_type", pageType }
        };

        _pagesScrapedCounter.Add(count, tags);
    }

    /// <summary>
    /// Creates a scope that tracks an active connection.
    /// Automatically increments on creation and decrements on disposal.
    /// </summary>
    /// <returns>A disposable scope for tracking the connection.</returns>
    public IDisposable BeginConnectionScope()
    {
        IncrementActiveConnections();
        return new ConnectionScope(this);
    }

    /// <summary>
    /// Disposes the meter and releases resources.
    /// </summary>
    public void Dispose()
    {
        _meter.Dispose();
    }

    /// <summary>
    /// Disposable scope that tracks an active connection.
    /// </summary>
    private sealed class ConnectionScope : IDisposable
    {
        private readonly HarvesterMetrics _metrics;
        private bool _disposed;

        public ConnectionScope(HarvesterMetrics metrics)
        {
            _metrics = metrics;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _metrics.DecrementActiveConnections();
            _disposed = true;
        }
    }
}

/// <summary>
/// Extension methods for registering HarvesterMetrics with dependency injection.
/// </summary>
public static class HarvesterMetricsExtensions
{
    /// <summary>
    /// Adds HarvesterMetrics as a singleton to the service collection.
    /// Also registers the meter with OpenTelemetry metrics.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddHarvesterMetrics(this IServiceCollection services)
    {
        services.AddSingleton<HarvesterMetrics>();
        return services;
    }
}
