#nullable enable

using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using JobCommandCenter.Harvester.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JobCommandCenter.Harvester.Services;

/// <summary>
/// Configuration options for Chrome DevTools Protocol connection.
/// </summary>
public sealed class CdpOptions
{
    /// <summary>
    /// Gets or sets the CDP endpoint URL.
    /// </summary>
    public string EndpointUrl { get; set; } = "http://localhost:9222";

    /// <summary>
    /// Gets or sets the connection timeout in seconds.
    /// </summary>
    public int ConnectionTimeoutSeconds { get; set; } = 30;
}

/// <summary>
/// Validates Chrome DevTools Protocol port availability using HTTP probe.
/// </summary>
public sealed class ChromeValidator : IChromeValidator
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ChromeValidator> _logger;
    private readonly CdpOptions _options;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    /// <summary>
    /// Creates a new instance of ChromeValidator.
    /// </summary>
    public ChromeValidator(
        IHttpClientFactory httpClientFactory,
        ILogger<ChromeValidator> logger,
        IOptions<CdpOptions> options)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public async Task<ChromeValidationResult> CheckPortAvailabilityAsync(CancellationToken cancellationToken = default)
    {
        var versionUrl = $"{_options.EndpointUrl.TrimEnd('/')}/json/version";
        
        _logger.LogDebug("Checking Chrome CDP availability at {Url}", versionUrl);

        try
        {
            using var httpClient = _httpClientFactory.CreateClient("ChromeValidator");
            httpClient.Timeout = TimeSpan.FromSeconds(Math.Min(_options.ConnectionTimeoutSeconds, 10));

            var response = await httpClient.GetAsync(versionUrl, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Chrome CDP endpoint returned status {StatusCode}",
                    response.StatusCode);

                return ChromeValidationResult.Failure(
                    $"Chrome returned HTTP {(int)response.StatusCode} ({response.StatusCode})",
                    GetGuidanceForStatus(response.StatusCode));
            }

            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var versionInfo = JsonSerializer.Deserialize<ChromeVersionInfo>(jsonContent, JsonOptions);

            if (versionInfo is null)
            {
                _logger.LogWarning("Chrome CDP endpoint returned invalid JSON response");
                return ChromeValidationResult.Failure(
                    "Chrome returned an invalid JSON response",
                    "Ensure Chrome is running with remote debugging enabled. Try restarting Chrome with --remote-debugging-port=9222");
            }

            _logger.LogInformation(
                "Chrome CDP available - Browser: {Browser}, User Data: {UserDataDir}",
                versionInfo.Browser,
                versionInfo.UserDataDir ?? "(default)");

            return ChromeValidationResult.Success(
                versionInfo.Browser ?? "Unknown",
                versionInfo.UserDataDir);
        }
        catch (HttpRequestException ex) when (ex.InnerException is System.Net.Sockets.SocketException)
        {
            _logger.LogWarning(ex, "Chrome CDP port is not reachable - connection refused");
            return CreateConnectionRefusedResult();
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("Connection refused", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(ex, "Chrome CDP port is not reachable - connection refused");
            return CreateConnectionRefusedResult();
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogWarning(ex, "Chrome CDP connection timed out");
            return ChromeValidationResult.Failure(
                "Connection to Chrome timed out",
                "The connection to Chrome took too long. Ensure Chrome is running with remote debugging enabled:\n\n" +
                "macOS: /Applications/Google\\ Chrome.app/Contents/MacOS/Google\\ Chrome --remote-debugging-port=9222\n" +
                "Linux: google-chrome --remote-debugging-port=9222\n" +
                "Windows: chrome.exe --remote-debugging-port=9222");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Chrome validation was cancelled");
            return ChromeValidationResult.Failure(
                "Operation was cancelled",
                "The validation request was cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error checking Chrome CDP availability");
            return ChromeValidationResult.Failure(
                $"Unexpected error: {ex.Message}",
                "An unexpected error occurred. Check the logs for details and ensure Chrome is running with:\n\n" +
                "--remote-debugging-port=9222");
        }
    }

    private static ChromeValidationResult CreateConnectionRefusedResult()
    {
        return ChromeValidationResult.Failure(
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

    private static string GetGuidanceForStatus(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.Unauthorized => "Chrome requires authentication. Check if another application is using port 9222.",
            HttpStatusCode.Forbidden => "Access to Chrome DevTools is forbidden. Check Chrome's security settings.",
            HttpStatusCode.NotFound => "The Chrome DevTools endpoint was not found. Ensure Chrome is running with --remote-debugging-port=9222",
            HttpStatusCode.ServiceUnavailable => "Chrome DevTools is temporarily unavailable. Try restarting Chrome.",
            _ => "Ensure Chrome is running with remote debugging enabled:\n\n" +
                  "macOS: /Applications/Google\\ Chrome.app/Contents/MacOS/Google\\ Chrome --remote-debugging-port=9222\n" +
                  "Linux: google-chrome --remote-debugging-port=9222\n" +
                  "Windows: chrome.exe --remote-debugging-port=9222"
        };
    }

    /// <summary>
    /// Represents the Chrome version information returned by /json/version endpoint.
    /// </summary>
    private sealed class ChromeVersionInfo
    {
        [JsonPropertyName("Browser")]
        public string? Browser { get; set; }

        [JsonPropertyName("Protocol-Version")]
        public string? ProtocolVersion { get; set; }

        [JsonPropertyName("User-Agent")]
        public string? UserAgent { get; set; }

        [JsonPropertyName("V8-Version")]
        public string? V8Version { get; set; }

        [JsonPropertyName("WebKit-Version")]
        public string? WebKitVersion { get; set; }

        [JsonPropertyName("webSocketDebuggerUrl")]
        public string? WebSocketDebuggerUrl { get; set; }

        [JsonPropertyName("user-data-dir")]
        public string? UserDataDir { get; set; }
    }
}
