using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using JobCommandCenter.Harvester.Configuration;

namespace JobCommandCenter.Harvester.Services;

/// <summary>
/// Validates Chrome remote debugging port availability using HTTP client.
/// </summary>
public class ChromeValidator : IChromeValidator
{
    private readonly HttpClient _httpClient;
    private readonly ChromeCdpOptions _options;
    private readonly ILogger<ChromeValidator> _logger;

    /// <summary>
    /// Initializes a new instance of the ChromeValidator class.
    /// </summary>
    /// <param name="httpClient">HTTP client for making requests.</param>
    /// <param name="options">Chrome CDP configuration options.</param>
    /// <param name="logger">Logger instance.</param>
    public ChromeValidator(
        HttpClient httpClient,
        ChromeCdpOptions options,
        ILogger<ChromeValidator> logger)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ChromeValidationResult> CheckPortAvailabilityAsync(CancellationToken cancellationToken = default)
    {
        var url = $"http://localhost:{_options.Port}/json/version";
        _logger.LogDebug("Checking Chrome CDP availability at {Url}", url);

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(_options.ConnectionTimeoutSeconds));

            var response = await _httpClient.GetAsync(url, cts.Token).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Chrome CDP endpoint returned status {StatusCode}", response.StatusCode);
                return CreateUnavailableResult(
                    $"Chrome CDP endpoint returned HTTP {(int)response.StatusCode} ({response.StatusCode})",
                    "Ensure Chrome is running with the correct remote debugging port.");
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            return ParseChromeVersionResponse(content);
        }
        catch (HttpRequestException ex) when (ex.InnerException is System.Net.Sockets.SocketException { SocketErrorCode: SocketError.ConnectionRefused })
        {
            _logger.LogWarning(ex, "Chrome CDP connection refused on port {Port}", _options.Port);
            return CreateUnavailableResult(
                $"Chrome is not running with remote debugging enabled on port {_options.Port}.",
                GetChromeStartCommand());
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Chrome CDP HTTP request failed on port {Port}", _options.Port);
            return CreateUnavailableResult(
                $"Failed to connect to Chrome CDP: {ex.Message}",
                GetChromeStartCommand());
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "Chrome CDP connection timed out after {Timeout} seconds", _options.ConnectionTimeoutSeconds);
            return CreateUnavailableResult(
                $"Connection to Chrome CDP timed out after {_options.ConnectionTimeoutSeconds} seconds.",
                "Chrome may be slow to respond. Try restarting Chrome with remote debugging.");
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse Chrome CDP version response");
            return CreateUnavailableResult(
                "Chrome CDP returned an invalid response format.",
                "Ensure you are using a compatible version of Chrome.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error checking Chrome CDP availability");
            return CreateUnavailableResult(
                $"Unexpected error: {ex.Message}",
                "Check that Chrome is properly installed and running.");
        }
    }

    private ChromeValidationResult ParseChromeVersionResponse(string content)
    {
        try
        {
            var versionInfo = JsonSerializer.Deserialize<ChromeVersionResponse>(content, JsonOptions);

            if (versionInfo is null)
            {
                _logger.LogWarning("Chrome CDP returned null version info");
                return CreateUnavailableResult(
                    "Chrome CDP returned an empty response.",
                    "Ensure you are using a compatible version of Chrome.");
            }

            _logger.LogInformation(
                "Chrome CDP available - Browser: {Browser}, Protocol: {Protocol}",
                versionInfo.Browser,
                versionInfo.WebSocketDebuggerUrl);

            return new ChromeValidationResult
            {
                IsAvailable = true,
                ChromeVersion = versionInfo.Browser,
                UserDataDirectory = versionInfo.UserDataDir
            };
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize Chrome version response: {Content}", content);
            return CreateUnavailableResult(
                "Chrome CDP returned an invalid JSON response.",
                "Ensure you are using a compatible version of Chrome.");
        }
    }

    private static ChromeValidationResult CreateUnavailableResult(string errorMessage, string resolutionGuidance)
    {
        return new ChromeValidationResult
        {
            IsAvailable = false,
            ErrorMessage = errorMessage,
            ResolutionGuidance = resolutionGuidance
        };
    }

    private string GetChromeStartCommand()
    {
        var port = _options.Port;
        return Environment.OSVersion.Platform switch
        {
            PlatformID.MacOSX => 
                $"/Applications/Google\\ Chrome.app/Contents/MacOS/Google\\ Chrome --remote-debugging-port={port}",
            PlatformID.Unix => 
                $"google-chrome --remote-debugging-port={port}",
            PlatformID.Win32NT => 
                $"chrome.exe --remote-debugging-port={port}",
            _ => 
                $"chrome --remote-debugging-port={port}"
        };
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    /// <summary>
    /// Response model for Chrome /json/version endpoint.
    /// </summary>
    private sealed class ChromeVersionResponse
    {
        /// <summary>
        /// Browser name and version.
        /// </summary>
        [JsonPropertyName("Browser")]
        public string? Browser { get; set; }

        /// <summary>
        /// WebSocket debugger URL.
        /// </summary>
        [JsonPropertyName("webSocketDebuggerUrl")]
        public string? WebSocketDebuggerUrl { get; set; }

        /// <summary>
        /// User data directory path.
        /// </summary>
        [JsonPropertyName("userDataDir")]
        public string? UserDataDir { get; set; }
    }
}
