using JobCommandCenter.Data;
using JobCommandCenter.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace JobCommandCenter.Harvester.Workers;

/// <summary>
/// Background worker that harvests job listings from LinkedIn via CDP.
/// </summary>
public class HarvestWorker : BackgroundService
{
    private readonly ILogger<HarvestWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ScoringEngine _scoringEngine;

    /// <summary>
    /// Chrome DevTools Protocol port.
    /// </summary>
    public const int ChromeDebugPort = 9222;

    /// <summary>
    /// Creates a new instance of the harvest worker.
    /// </summary>
    public HarvestWorker(
        ILogger<HarvestWorker> logger,
        IServiceProvider serviceProvider,
        ScoringEngine scoringEngine)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _scoringEngine = scoringEngine;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("HarvestWorker starting. Chrome CDP port: {Port}", ChromeDebugPort);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // TODO: Implement CDP connection and scraping logic
                // For now, just log a heartbeat
                _logger.LogDebug("HarvestWorker heartbeat at: {Time}", DateTimeOffset.Now);

                // Human-like delay between cycles
                var delay = Random.Shared.Next(30000, 60000); // 30-60 seconds
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected during shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in harvest cycle");
                await Task.Delay(5000, stoppingToken); // Brief delay before retry
            }
        }

        _logger.LogInformation("HarvestWorker stopping");
    }
}
