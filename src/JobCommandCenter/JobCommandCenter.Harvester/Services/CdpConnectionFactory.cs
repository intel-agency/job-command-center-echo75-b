#nullable enable

using System.Diagnostics;
using JobCommandCenter.Harvester.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;

namespace JobCommandCenter.Harvester.Services;

/// <summary>
/// Factory for establishing Chrome DevTools Protocol connections using Playwright.
/// </summary>
public sealed class CdpConnectionFactory : ICdpConnectionFactory
{
    private readonly IChromeValidator _chromeValidator;
    private readonly ILogger<CdpConnectionFactory> _logger;
    private readonly CdpOptions _options;
    private readonly IPlaywright? _playwright;

    /// <summary>
    /// Creates a new instance of CdpConnectionFactory.
    /// </summary>
    /// <param name="chromeValidator">The Chrome validator for pre-flight checks.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="options">The CDP configuration options.</param>
    /// <param name="playwright">Optional Playwright instance (for testing purposes).</param>
    public CdpConnectionFactory(
        IChromeValidator chromeValidator,
        ILogger<CdpConnectionFactory> logger,
        IOptions<CdpOptions> options,
        IPlaywright? playwright = null)
    {
        _chromeValidator = chromeValidator ?? throw new ArgumentNullException(nameof(chromeValidator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _playwright = playwright;
    }

    /// <inheritdoc />
    public async Task<CdpConnectionResult> ConnectAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Starting CDP connection to {Endpoint}", _options.EndpointUrl);

        // Pre-flight validation
        var validationResult = await _chromeValidator.CheckPortAvailabilityAsync(cancellationToken).ConfigureAwait(false);
        if (!validationResult.IsAvailable)
        {
            _logger.LogWarning("CDP pre-flight validation failed: {Error}", validationResult.ErrorMessage);
            return CdpConnectionResult.Failure(
                $"Pre-flight validation failed: {validationResult.ErrorMessage}",
                validationResult.GuidanceMessage ?? GetDefaultGuidance());
        }

        _logger.LogDebug("Pre-flight validation passed - Chrome version: {Version}, UserDataDir: {UserDataDir}",
            validationResult.ChromeVersion, validationResult.UserDataDir ?? "(default)");

        // Attempt CDP connection
        try
        {
            var playwright = _playwright ?? await Playwright.CreateAsync().ConfigureAwait(false);
            var timeoutMs = _options.ConnectionTimeoutSeconds * 1000;

            _logger.LogDebug("Connecting via CDP with timeout {Timeout}ms", timeoutMs);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromMilliseconds(timeoutMs));

            IBrowser browser;
            try
            {
                browser = await playwright.Chromium.ConnectOverCDPAsync(
                    _options.EndpointUrl,
                    new BrowserTypeConnectOverCDPOptions
                    {
                        Timeout = timeoutMs
                    }).WaitAsync(cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // This was a timeout, not user cancellation
                _logger.LogWarning("CDP connection timed out after {Timeout}ms", timeoutMs);
                return CdpConnectionResult.Failure(
                    $"Connection timed out after {_options.ConnectionTimeoutSeconds} seconds",
                    "The connection to Chrome took too long. Ensure Chrome is running with remote debugging enabled:\n\n" +
                    "macOS: /Applications/Google\\ Chrome.app/Contents/MacOS/Google\\ Chrome --remote-debugging-port=9222\n" +
                    "Linux: google-chrome --remote-debugging-port=9222\n" +
                    "Windows: chrome.exe --remote-debugging-port=9222\n\n" +
                    "Also check your network connection and firewall settings.");
            }

            // Validate the connected session
            var browserVersion = browser.Version;
            var contexts = browser.Contexts;

            _logger.LogInformation(
                "CDP connection established - Version: {Version}, Contexts: {ContextCount}",
                browserVersion,
                contexts.Count);

            stopwatch.Stop();
            _logger.LogInformation("CDP connection completed in {Duration}ms", stopwatch.ElapsedMilliseconds);

            return CdpConnectionResult.Success(
                browser,
                browserVersion,
                validationResult.UserDataDir);
        }
        catch (PlaywrightException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Playwright error during CDP connection");
            return HandlePlaywrightException(ex);
        }
        catch (TimeoutException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Timeout during CDP connection");
            return CdpConnectionResult.Failure(
                "Connection timed out",
                "The connection to Chrome timed out. Ensure Chrome is running with remote debugging enabled:\n\n" +
                "macOS: /Applications/Google\\ Chrome.app/Contents/MacOS/Google\\ Chrome --remote-debugging-port=9222\n" +
                "Linux: google-chrome --remote-debugging-port=9222\n" +
                "Windows: chrome.exe --remote-debugging-port=9222");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            _logger.LogInformation("CDP connection was cancelled by user");
            return CdpConnectionResult.Failure(
                "Operation was cancelled",
                "The connection request was cancelled.");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Unexpected error during CDP connection");
            return CdpConnectionResult.Failure(
                $"Unexpected error: {ex.Message}",
                GetDefaultGuidance());
        }
    }

    /// <summary>
    /// Handles Playwright exceptions and converts them to user-friendly error results.
    /// </summary>
    private CdpConnectionResult HandlePlaywrightException(PlaywrightException ex)
    {
        var message = ex.Message.ToLowerInvariant();

        if (message.Contains("connection refused") || message.Contains("econnrefused"))
        {
            return CdpConnectionResult.Failure(
                "Connection refused - Chrome is not running or remote debugging is disabled",
                "Start Chrome with remote debugging enabled:\n\n" +
                "macOS:\n" +
                "  /Applications/Google\\ Chrome.app/Contents/MacOS/Google\\ Chrome --remote-debugging-port=9222\n\n" +
                "Linux:\n" +
                "  google-chrome --remote-debugging-port=9222\n\n" +
                "Windows:\n" +
                "  chrome.exe --remote-debugging-port=9222\n\n" +
                "Note: Close all Chrome instances before launching with this flag to ensure it takes effect.");
        }

        if (message.Contains("timeout") || message.Contains("timed out"))
        {
            return CdpConnectionResult.Failure(
                "Connection timed out",
                "The connection to Chrome took too long. Ensure Chrome is running with remote debugging enabled:\n\n" +
                "macOS: /Applications/Google\\ Chrome.app/Contents/MacOS/Google\\ Chrome --remote-debugging-port=9222\n" +
                "Linux: google-chrome --remote-debugging-port=9222\n" +
                "Windows: chrome.exe --remote-debugging-port=9222\n\n" +
                "Also check your network connection and firewall settings.");
        }

        if (message.Contains("invalid endpoint") || message.Contains("invalid url"))
        {
            return CdpConnectionResult.Failure(
                $"Invalid CDP endpoint URL: {_options.EndpointUrl}",
                "The configured CDP endpoint URL is invalid. Check the 'Cdp:EndpointUrl' configuration value.\n\n" +
                "Expected format: http://localhost:9222 or http://hostname:port");
        }

        if (message.Contains("unauthorized") || message.Contains("forbidden"))
        {
            return CdpConnectionResult.Failure(
                "Access to Chrome DevTools was denied",
                "Chrome requires authentication or access is forbidden. Check if another application is using port 9222 " +
                "or if Chrome's security settings are blocking the connection.");
        }

        if (message.Contains("target closed") || message.Contains("disconnected"))
        {
            return CdpConnectionResult.Failure(
                "Chrome connection was closed unexpectedly",
                "Chrome closed the connection. This may happen if Chrome was restarted or crashed.\n\n" +
                "Try restarting Chrome with remote debugging enabled:\n" +
                "--remote-debugging-port=9222");
        }

        // Generic Playwright error
        return CdpConnectionResult.Failure(
            $"Playwright error: {ex.Message}",
            GetDefaultGuidance());
    }

    /// <summary>
    /// Gets the default guidance message for connection issues.
    /// </summary>
    private static string GetDefaultGuidance()
    {
        return "Ensure Chrome is running with remote debugging enabled:\n\n" +
               "macOS:\n" +
               "  /Applications/Google\\ Chrome.app/Contents/MacOS/Google\\ Chrome --remote-debugging-port=9222\n\n" +
               "Linux:\n" +
               "  google-chrome --remote-debugging-port=9222\n\n" +
               "Windows:\n" +
               "  chrome.exe --remote-debugging-port=9222\n\n" +
               "Note: Close all Chrome instances before launching with this flag to ensure it takes effect.";
    }
}
