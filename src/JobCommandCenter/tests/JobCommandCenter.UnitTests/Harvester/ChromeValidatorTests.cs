using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.Json;
using JobCommandCenter.Harvester.Configuration;
using JobCommandCenter.Harvester.Services;
using Microsoft.Extensions.Logging;

namespace JobCommandCenter.UnitTests.Harvester;

/// <summary>
/// Unit tests for the ChromeValidator class.
/// </summary>
public class ChromeValidatorTests
{
    private readonly ChromeCdpOptions _defaultOptions;
    private readonly ILogger<ChromeValidator> _logger;

    public ChromeValidatorTests()
    {
        _defaultOptions = new ChromeCdpOptions
        {
            Port = 9222,
            ConnectionTimeoutSeconds = 5
        };
        _logger = new LoggerFactory().CreateLogger<ChromeValidator>();
    }

    #region Chrome Available Tests

    [Fact]
    public async Task CheckPortAvailabilityAsync_WhenChromeAvailable_ReturnsSuccessResult()
    {
        // Arrange
        var chromeVersion = "Chrome/120.0.6099.109";
        var userDataDir = "/tmp/chrome-user-data";
        var responseJson = CreateChromeVersionResponse(chromeVersion, userDataDir);
        
        var handler = new MockHttpMessageHandler(responseJson, HttpStatusCode.OK);
        var httpClient = new HttpClient(handler);
        var validator = new ChromeValidator(httpClient, _defaultOptions, _logger);

        // Act
        var result = await validator.CheckPortAvailabilityAsync();

        // Assert
        result.IsAvailable.Should().BeTrue();
        result.ChromeVersion.Should().Be(chromeVersion);
        result.UserDataDirectory.Should().Be(userDataDir);
        result.ErrorMessage.Should().BeNull();
        result.ResolutionGuidance.Should().BeNull();
    }

    [Fact]
    public async Task CheckPortAvailabilityAsync_WhenChromeAvailable_MakesCorrectRequest()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(
            CreateChromeVersionResponse("Chrome/120.0.0.0", "/tmp"),
            HttpStatusCode.OK);
        var httpClient = new HttpClient(handler);
        var validator = new ChromeValidator(httpClient, _defaultOptions, _logger);

        // Act
        await validator.CheckPortAvailabilityAsync();

        // Assert
        handler.LastRequestUri.Should().Be("http://localhost:9222/json/version");
    }

    [Fact]
    public async Task CheckPortAvailabilityAsync_WithCustomPort_UsesCorrectPort()
    {
        // Arrange
        var options = new ChromeCdpOptions { Port = 9333, ConnectionTimeoutSeconds = 5 };
        var handler = new MockHttpMessageHandler(
            CreateChromeVersionResponse("Chrome/120.0.0.0", "/tmp"),
            HttpStatusCode.OK);
        var httpClient = new HttpClient(handler);
        var validator = new ChromeValidator(httpClient, options, _logger);

        // Act
        await validator.CheckPortAvailabilityAsync();

        // Assert
        handler.LastRequestUri.Should().Be("http://localhost:9333/json/version");
    }

    #endregion

    #region Chrome Not Running Tests

    [Fact]
    public async Task CheckPortAvailabilityAsync_WhenConnectionRefused_ReturnsUnavailableWithGuidance()
    {
        // Arrange
        var socketException = new SocketException((int)SocketError.ConnectionRefused);
        var httpRequestException = new HttpRequestException("Connection refused", socketException);
        var handler = new MockHttpMessageHandler(httpRequestException);
        var httpClient = new HttpClient(handler);
        var validator = new ChromeValidator(httpClient, _defaultOptions, _logger);

        // Act
        var result = await validator.CheckPortAvailabilityAsync();

        // Assert
        result.IsAvailable.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Chrome is not running");
        result.ErrorMessage.Should().Contain("9222");
        result.ResolutionGuidance.Should().Contain("--remote-debugging-port");
    }

    [Fact]
    public async Task CheckPortAvailabilityAsync_WhenConnectionRefused_IncludesPlatformSpecificCommand()
    {
        // Arrange
        var socketException = new SocketException((int)SocketError.ConnectionRefused);
        var httpRequestException = new HttpRequestException("Connection refused", socketException);
        var handler = new MockHttpMessageHandler(httpRequestException);
        var httpClient = new HttpClient(handler);
        var validator = new ChromeValidator(httpClient, _defaultOptions, _logger);

        // Act
        var result = await validator.CheckPortAvailabilityAsync();

        // Assert
        result.ResolutionGuidance.Should().Contain("chrome");
        result.ResolutionGuidance.Should().Contain("--remote-debugging-port=9222");
    }

    #endregion

    #region HTTP Error Tests

    [Fact]
    public async Task CheckPortAvailabilityAsync_WhenHttpError_ReturnsUnavailableWithGuidance()
    {
        // Arrange
        var handler = new MockHttpMessageHandler("Internal Server Error", HttpStatusCode.InternalServerError);
        var httpClient = new HttpClient(handler);
        var validator = new ChromeValidator(httpClient, _defaultOptions, _logger);

        // Act
        var result = await validator.CheckPortAvailabilityAsync();

        // Assert
        result.IsAvailable.Should().BeFalse();
        result.ErrorMessage.Should().Contain("500");
        result.ResolutionGuidance.Should().Contain("remote debugging port");
    }

    [Fact]
    public async Task CheckPortAvailabilityAsync_WhenNotFound_ReturnsUnavailableWithGuidance()
    {
        // Arrange
        var handler = new MockHttpMessageHandler("Not Found", HttpStatusCode.NotFound);
        var httpClient = new HttpClient(handler);
        var validator = new ChromeValidator(httpClient, _defaultOptions, _logger);

        // Act
        var result = await validator.CheckPortAvailabilityAsync();

        // Assert
        result.IsAvailable.Should().BeFalse();
        result.ErrorMessage.Should().Contain("404");
    }

    [Fact]
    public async Task CheckPortAvailabilityAsync_WhenUnauthorized_ReturnsUnavailableWithGuidance()
    {
        // Arrange
        var handler = new MockHttpMessageHandler("Unauthorized", HttpStatusCode.Unauthorized);
        var httpClient = new HttpClient(handler);
        var validator = new ChromeValidator(httpClient, _defaultOptions, _logger);

        // Act
        var result = await validator.CheckPortAvailabilityAsync();

        // Assert
        result.IsAvailable.Should().BeFalse();
        result.ErrorMessage.Should().Contain("401");
    }

    #endregion

    #region Timeout Tests

    [Fact]
    public async Task CheckPortAvailabilityAsync_WhenTimeout_ReturnsUnavailableWithGuidance()
    {
        // Arrange
        var handler = new MockHttpMessageHandlerWithDelay(TimeSpan.FromSeconds(10));
        var httpClient = new HttpClient(handler);
        var options = new ChromeCdpOptions { Port = 9222, ConnectionTimeoutSeconds = 1 };
        var validator = new ChromeValidator(httpClient, options, _logger);

        // Act
        var result = await validator.CheckPortAvailabilityAsync();

        // Assert
        result.IsAvailable.Should().BeFalse();
        result.ErrorMessage.Should().Contain("timed out");
        result.ErrorMessage.Should().Contain("1 second");
        result.ResolutionGuidance.Should().Contain("restart");
    }

    #endregion

    #region Invalid JSON Response Tests

    [Fact]
    public async Task CheckPortAvailabilityAsync_WhenInvalidJson_ReturnsUnavailableWithGuidance()
    {
        // Arrange
        var handler = new MockHttpMessageHandler("not valid json", HttpStatusCode.OK);
        var httpClient = new HttpClient(handler);
        var validator = new ChromeValidator(httpClient, _defaultOptions, _logger);

        // Act
        var result = await validator.CheckPortAvailabilityAsync();

        // Assert
        result.IsAvailable.Should().BeFalse();
        result.ErrorMessage.Should().Contain("invalid");
        result.ResolutionGuidance.Should().Contain("compatible version");
    }

    [Fact]
    public async Task CheckPortAvailabilityAsync_WhenEmptyResponse_ReturnsUnavailableWithGuidance()
    {
        // Arrange
        var handler = new MockHttpMessageHandler("", HttpStatusCode.OK);
        var httpClient = new HttpClient(handler);
        var validator = new ChromeValidator(httpClient, _defaultOptions, _logger);

        // Act
        var result = await validator.CheckPortAvailabilityAsync();

        // Assert
        result.IsAvailable.Should().BeFalse();
        result.ErrorMessage.Should().Contain("invalid");
    }

    [Fact]
    public async Task CheckPortAvailabilityAsync_WhenNullJson_ReturnsUnavailableWithGuidance()
    {
        // Arrange
        var handler = new MockHttpMessageHandler("null", HttpStatusCode.OK);
        var httpClient = new HttpClient(handler);
        var validator = new ChromeValidator(httpClient, _defaultOptions, _logger);

        // Act
        var result = await validator.CheckPortAvailabilityAsync();

        // Assert
        result.IsAvailable.Should().BeFalse();
        result.ErrorMessage.Should().Contain("empty response");
    }

    [Fact]
    public async Task CheckPortAvailabilityAsync_WhenMissingRequiredFields_ReturnsSuccessWithNullFields()
    {
        // Arrange - response with only some fields
        var responseJson = @"{""webSocketDebuggerUrl"":""ws://localhost:9222""}";
        var handler = new MockHttpMessageHandler(responseJson, HttpStatusCode.OK);
        var httpClient = new HttpClient(handler);
        var validator = new ChromeValidator(httpClient, _defaultOptions, _logger);

        // Act
        var result = await validator.CheckPortAvailabilityAsync();

        // Assert
        result.IsAvailable.Should().BeTrue();
        result.ChromeVersion.Should().BeNull();
        result.UserDataDirectory.Should().BeNull();
    }

    #endregion

    #region Helper Methods

    private static string CreateChromeVersionResponse(string browser, string userDataDir)
    {
        var response = new
        {
            Browser = browser,
            ProtocolVersion = "1.3",
            WebVersion = "1.3",
            webSocketDebuggerUrl = "ws://localhost:9222/devtools/browser/xxx",
            userDataDir = userDataDir
        };
        return JsonSerializer.Serialize(response);
    }

    #endregion

    #region Mock Handlers

    private sealed class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly string? _response;
        private readonly HttpStatusCode _statusCode;
        private readonly Exception? _exception;

        public string? LastRequestUri { get; private set; }

        public MockHttpMessageHandler(string response, HttpStatusCode statusCode)
        {
            _response = response;
            _statusCode = statusCode;
        }

        public MockHttpMessageHandler(Exception exception)
        {
            _exception = exception;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequestUri = request.RequestUri?.ToString();

            if (_exception is not null)
            {
                throw _exception;
            }

            return Task.FromResult(new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_response ?? string.Empty)
            });
        }
    }

    private sealed class MockHttpMessageHandlerWithDelay : HttpMessageHandler
    {
        private readonly TimeSpan _delay;

        public MockHttpMessageHandlerWithDelay(TimeSpan delay)
        {
            _delay = delay;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            await Task.Delay(_delay, cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}")
            };
        }
    }

    #endregion
}
