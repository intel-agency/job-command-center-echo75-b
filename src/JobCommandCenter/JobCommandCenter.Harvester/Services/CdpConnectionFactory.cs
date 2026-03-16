#nullable enable

using JobCommandCenter.Harvester.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace JobCommandCenter.Harvester.Services;

/// <summary>
/// Factory for creating Chrome DevTools Protocol browser connections using Playwright.
/// </summary>
public sealed class CdpConnectionFactory : ICdpConnectionFactory
{
    private readonly IPlaywright _playwright;
    private readonly ChromeCdpOptions _options;
    private readonly ILogger<CdpConnectionFactory> _logger;

    /// <summary>
    /// Creates a new instance of CdpConnectionFactory.
    /// </summary>
    /// <param name="playwright">The Playwright instance.</param>
    /// <param name="options">Chrome CDP configuration options.</param>
    /// <param name="logger">Logger instance.</param>
    public CdpConnectionFactory(
        IPlaywright playwright,
        ChromeCdpOptions options,
        ILogger<CdpConnectionFactory> logger)
    {
        _playwright = playwright ?? throw new ArgumentNullException(nameof(playwright));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IBrowser> ConnectAsync(CancellationToken cancellationToken = default)
    {
        var endpointUrl = $"http://localhost:{_options.Port}";
        var timeout = TimeSpan.FromSeconds(_options.ConnectionTimeoutSeconds);

        _logger.LogInformation(
            "Connecting to Chrome CDP at {Endpoint} with timeout {Timeout}s",
            endpointUrl,
            _options.ConnectionTimeoutSeconds);

        using var timeoutCts = new CancellationTokenSource(timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            timeoutCts.Token);

        try
        {
            var browser = await _playwright.Chromium.ConnectOverCDPAsync(endpointUrl)
                .WaitAsync(linkedCts.Token)
                .ConfigureAwait(false);

            // Validate the connection by checking browser version and contexts
            await ValidateConnectionAsync(browser, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation(
                "Successfully connected to Chrome - Version: {BrowserVersion}, Contexts: {ContextCount}",
                browser.Version,
                browser.Contexts.Count);

            return browser;
        }
        catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
        {
            _logger.LogError(
                "CDP connection to {Endpoint} timed out after {Timeout}s",
                endpointUrl,
                _options.ConnectionTimeoutSeconds);

            throw new TimeoutException(
                $"Connection to Chrome at {endpointUrl} timed out after {_options.ConnectionTimeoutSeconds} seconds. " +
                "Ensure Chrome is running with remote debugging enabled:\n\n" +
                "macOS: /Applications/Google\\ Chrome.app/Contents/MacOS/Google\\ Chrome --remote-debugging-port=9222\n" +
                "Linux: google-chrome --remote-debugging-port=9222\n" +
                "Windows: chrome.exe --remote-debugging-port=9222");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("CDP connection was cancelled by caller");
            throw;
        }
        catch (PlaywrightException ex) when (ex.Message.Contains("ECONNREFUSED", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogError(ex, "Chrome CDP connection refused at {Endpoint}", endpointUrl);

            throw new InvalidOperationException(
                $"Connection refused - Chrome is not running or remote debugging is disabled on port {_options.Port}.\n\n" +
                "Start Chrome with remote debugging enabled:\n\n" +
                "macOS:\n" +
                "  /Applications/Google\\ Chrome.app/Contents/MacOS/Google\\ Chrome --remote-debugging-port=9222\n\n" +
                "Linux:\n" +
                "  google-chrome --remote-debugging-port=9222\n\n" +
                "Windows:\n" +
                "  chrome.exe --remote-debugging-port=9222\n\n" +
                "Note: Close all Chrome instances before launching with this flag to ensure it takes effect.",
                ex);
        }
        catch (PlaywrightException ex) when (ex.Message.Contains("Invalid URL", StringComparison.OrdinalIgnoreCase) ||
                                              ex.Message.Contains("Invalid endpoint", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogError(ex, "Invalid CDP endpoint URL: {Endpoint}", endpointUrl);

            throw new ArgumentException(
                $"Invalid CDP endpoint URL: {endpointUrl}. " +
                "Ensure the port number is valid and the endpoint is accessible.",
                ex);
        }
        catch (PlaywrightException ex)
        {
            _logger.LogError(ex, "Failed to connect to Chrome CDP at {Endpoint}", endpointUrl);

            throw new InvalidOperationException(
                $"Failed to connect to Chrome at {endpointUrl}: {ex.Message}\n\n" +
                "Ensure Chrome is running with remote debugging enabled:\n\n" +
                "macOS: /Applications/Google\\ Chrome.app/Contents/MacOS/Google\\ Chrome --remote-debugging-port=9222\n" +
                "Linux: google-chrome --remote-debugging-port=9222\n" +
                "Windows: chrome.exe --remote-debugging-port=9222",
                ex);
        }
    }

    private async Task ValidateConnectionAsync(IBrowser browser, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Validating CDP connection...");

        // Check browser version
        var version = browser.Version;
        if (string.IsNullOrEmpty(version))
        {
            _logger.LogWarning("Browser version is empty - connection may be unstable");
        }
        else
        {
            _logger.LogDebug("Browser version: {BrowserVersion}", version);
        }

        // Check browser contexts - should have at least the default context
        var contexts = browser.Contexts;
        if (contexts.Count == 0)
        {
            _logger.LogWarning("No browser contexts available - connection may be incomplete");
        }
        else
        {
            _logger.LogDebug("Browser has {ContextCount} context(s)", contexts.Count);

            // Log default context info if available
            var defaultContext = contexts.FirstOrDefault();
            if (defaultContext != null)
            {
                var pages = defaultContext.Pages;
                _logger.LogDebug("Default context has {PageCount} page(s)", pages.Count);
            }
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }
}
