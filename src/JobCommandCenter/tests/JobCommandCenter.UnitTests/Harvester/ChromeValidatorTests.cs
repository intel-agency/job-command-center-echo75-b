#nullable enable

using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using FluentAssertions;
using JobCommandCenter.Harvester.Models;
using JobCommandCenter.Harvester.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace JobCommandCenter.UnitTests.Harvester;

/// <summary>
/// Unit tests for ChromeValidator pre-flight validation functionality.
/// </summary>
public class ChromeValidatorTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<ILogger<ChromeValidator>> _loggerMock;
    private readonly CdpOptions _options;

    public ChromeValidatorTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _loggerMock = new Mock<ILogger<ChromeValidator>>();
        _options = new CdpOptions
        {
            EndpointUrl = "http://localhost:9222",
            ConnectionTimeoutSeconds = 30
        };
    }

    private ChromeValidator CreateValidator(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler);
        _httpClientFactoryMock
            .Setup(f => f.CreateClient("ChromeValidator"))
            .Returns(httpClient);

        var optionsMock = Options.Create(_options);
        return new ChromeValidator(_httpClientFactoryMock.Object, _loggerMock.Object, optionsMock);
    }

    #region Success Scenarios

    [Fact]
    public async Task CheckPortAvailabilityAsync_WhenChromeRunning_ReturnsAvailable()
    {
        // Arrange - using raw JSON to match Chrome's actual response format with hyphenated property names
        var json = """
        {
            "Browser": "HeadlessChrome/120.0.6099.109",
            "Protocol-Version": "1.3",
            "User-Agent": "Mozilla/5.0...",
            "V8-Version": "12.0.267.8",
            "WebKit-Version": "537.36",
            "webSocketDebuggerUrl": "ws://localhost:9222/devtools/browser/xxx",
            "user-data-dir": "/tmp/chrome-profile"
        }
        """;
        
        var handler = CreateMockHandler(HttpStatusCode.OK, json);
        var validator = CreateValidator(handler);

        // Act
        var result = await validator.CheckPortAvailabilityAsync();

        // Assert
        result.IsAvailable.Should().BeTrue();
        result.ChromeVersion.Should().Be("HeadlessChrome/120.0.6099.109");
        result.UserDataDir.Should().Be("/tmp/chrome-profile");
        result.ErrorMessage.Should().BeNull();
        result.GuidanceMessage.Should().BeNull();
    }

    [Fact]
    public async Task CheckPortAvailabilityAsync_WhenChromeRunningWithoutUserDataDir_ReturnsAvailable()
    {
        // Arrange
        var versionResponse = new
        {
            Browser = "Chrome/119.0.6045.105",
            Protocol_Version = "1.3",
            webSocketDebuggerUrl = "ws://localhost:9222/devtools/browser/yyy"
        };

        var json = JsonSerializer.Serialize(versionResponse);
        var handler = CreateMockHandler(HttpStatusCode.OK, json);
        var validator = CreateValidator(handler);

        // Act
        var result = await validator.CheckPortAvailabilityAsync();

        // Assert
        result.IsAvailable.Should().BeTrue();
        result.ChromeVersion.Should().Be("Chrome/119.0.6045.105");
        result.UserDataDir.Should().BeNull();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task CheckPortAvailabilityAsync_WithCustomEndpoint_UsesCorrectUrl()
    {
        // Arrange
        _options.EndpointUrl = "http://custom-host:8080";
        
        var versionResponse = new { Browser = "Chrome/120.0.0.0" };
        var json = JsonSerializer.Serialize(versionResponse);
        
        HttpRequestMessage? capturedRequest = null;
        var handler = CreateMockHandler(HttpStatusCode.OK, json, r => capturedRequest = r);
        var validator = CreateValidator(handler);

        // Act
        var result = await validator.CheckPortAvailabilityAsync();

        // Assert
        result.IsAvailable.Should().BeTrue();
        capturedRequest.Should().NotBeNull();
        capturedRequest!.RequestUri!.ToString().Should().Be("http://custom-host:8080/json/version");
    }

    #endregion

    #region Connection Refused Scenarios

    [Fact]
    public async Task CheckPortAvailabilityAsync_WhenChromeNotRunning_ReturnsUnavailable()
    {
        // Arrange
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused", 
                new SocketException((int)SocketError.ConnectionRefused)));

        var validator = CreateValidator(handler.Object);

        // Act
        var result = await validator.CheckPortAvailabilityAsync();

        // Assert
        result.IsAvailable.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNull();
        result.ErrorMessage.Should().Contain("Connection refused");
        result.GuidanceMessage.Should().NotBeNull();
        result.GuidanceMessage.Should().Contain("--remote-debugging-port=9222");
    }

    [Fact]
    public async Task CheckPortAvailabilityAsync_WhenChromeNotRunning_ContainsMacOsGuidance()
    {
        // Arrange
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused", 
                new SocketException((int)SocketError.ConnectionRefused)));

        var validator = CreateValidator(handler.Object);

        // Act
        var result = await validator.CheckPortAvailabilityAsync();

        // Assert
        result.GuidanceMessage.Should().Contain("macOS");
        result.GuidanceMessage.Should().Contain("Google\\ Chrome.app");
    }

    [Fact]
    public async Task CheckPortAvailabilityAsync_WhenChromeNotRunning_ContainsLinuxGuidance()
    {
        // Arrange
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused", 
                new SocketException((int)SocketError.ConnectionRefused)));

        var validator = CreateValidator(handler.Object);

        // Act
        var result = await validator.CheckPortAvailabilityAsync();

        // Assert
        result.GuidanceMessage.Should().Contain("Linux");
        result.GuidanceMessage.Should().Contain("google-chrome");
    }

    [Fact]
    public async Task CheckPortAvailabilityAsync_WhenChromeNotRunning_ContainsWindowsGuidance()
    {
        // Arrange
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused", 
                new SocketException((int)SocketError.ConnectionRefused)));

        var validator = CreateValidator(handler.Object);

        // Act
        var result = await validator.CheckPortAvailabilityAsync();

        // Assert
        result.GuidanceMessage.Should().Contain("Windows");
        result.GuidanceMessage.Should().Contain("chrome.exe");
    }

    #endregion

    #region Timeout Scenarios

    [Fact]
    public async Task CheckPortAvailabilityAsync_WhenTimeout_ReturnsUnavailable()
    {
        // Arrange
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("The operation was canceled.", 
                new TimeoutException()));

        var validator = CreateValidator(handler.Object);

        // Act
        var result = await validator.CheckPortAvailabilityAsync();

        // Assert
        result.IsAvailable.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNull();
        result.ErrorMessage.Should().Contain("timed out");
        result.GuidanceMessage.Should().NotBeNull();
    }

    [Fact]
    public async Task CheckPortAvailabilityAsync_WhenTimeout_ContainsGuidance()
    {
        // Arrange
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("The operation was canceled.", 
                new TimeoutException()));

        var validator = CreateValidator(handler.Object);

        // Act
        var result = await validator.CheckPortAvailabilityAsync();

        // Assert
        result.GuidanceMessage.Should().Contain("--remote-debugging-port=9222");
    }

    #endregion

    #region Invalid Response Scenarios

    [Fact]
    public async Task CheckPortAvailabilityAsync_WhenInvalidJson_ReturnsUnavailable()
    {
        // Arrange
        var handler = CreateMockHandler(HttpStatusCode.OK, "not valid json");
        var validator = CreateValidator(handler);

        // Act
        var result = await validator.CheckPortAvailabilityAsync();

        // Assert
        result.IsAvailable.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNull();
        result.ErrorMessage.Should().Contain("invalid");
    }

    [Fact]
    public async Task CheckPortAvailabilityAsync_WhenEmptyResponse_ReturnsUnavailable()
    {
        // Arrange
        var handler = CreateMockHandler(HttpStatusCode.OK, "");
        var validator = CreateValidator(handler);

        // Act
        var result = await validator.CheckPortAvailabilityAsync();

        // Assert
        result.IsAvailable.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNull();
    }

    [Fact]
    public async Task CheckPortAvailabilityAsync_WhenHttp404_ReturnsUnavailable()
    {
        // Arrange
        var handler = CreateMockHandler(HttpStatusCode.NotFound, "Not Found");
        var validator = CreateValidator(handler);

        // Act
        var result = await validator.CheckPortAvailabilityAsync();

        // Assert
        result.IsAvailable.Should().BeFalse();
        result.ErrorMessage.Should().Contain("404");
    }

    [Fact]
    public async Task CheckPortAvailabilityAsync_WhenHttp500_ReturnsUnavailable()
    {
        // Arrange
        var handler = CreateMockHandler(HttpStatusCode.InternalServerError, "Internal Server Error");
        var validator = CreateValidator(handler);

        // Act
        var result = await validator.CheckPortAvailabilityAsync();

        // Assert
        result.IsAvailable.Should().BeFalse();
        result.ErrorMessage.Should().Contain("500");
    }

    [Fact]
    public async Task CheckPortAvailabilityAsync_WhenHttp401_ReturnsUnavailable()
    {
        // Arrange
        var handler = CreateMockHandler(HttpStatusCode.Unauthorized, "Unauthorized");
        var validator = CreateValidator(handler);

        // Act
        var result = await validator.CheckPortAvailabilityAsync();

        // Assert
        result.IsAvailable.Should().BeFalse();
        result.ErrorMessage.Should().Contain("401");
    }

    #endregion

    #region Cancellation Scenarios

    [Fact]
    public async Task CheckPortAvailabilityAsync_WhenCancelled_ReturnsFailure()
    {
        // Arrange
        var handler = new Mock<HttpMessageHandler>();
        var cts = new CancellationTokenSource();
        
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((_, ct) =>
            {
                cts.Cancel();
                ct.ThrowIfCancellationRequested();
            })
            .ThrowsAsync(new OperationCanceledException());

        var validator = CreateValidator(handler.Object);

        // Act
        var result = await validator.CheckPortAvailabilityAsync(cts.Token);

        // Assert
        result.IsAvailable.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNull();
    }

    #endregion

    #region Helper Methods

    private static HttpMessageHandler CreateMockHandler(
        HttpStatusCode statusCode, 
        string content,
        Action<HttpRequestMessage>? captureRequest = null)
    {
        var handler = new Mock<HttpMessageHandler>();
        
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) => captureRequest?.Invoke(request))
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json")
            });

        return handler.Object;
    }

    #endregion
}
