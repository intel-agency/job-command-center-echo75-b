#nullable enable

using FluentAssertions;
using JobCommandCenter.Harvester.Models;
using JobCommandCenter.Harvester.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using Moq;
using Xunit;

namespace JobCommandCenter.UnitTests.Harvester;

/// <summary>
/// Unit tests for CdpConnectionFactory CDP connection functionality.
/// </summary>
public class CdpConnectionFactoryTests
{
    private readonly Mock<IChromeValidator> _chromeValidatorMock;
    private readonly Mock<ILogger<CdpConnectionFactory>> _loggerMock;
    private readonly CdpOptions _options;

    public CdpConnectionFactoryTests()
    {
        _chromeValidatorMock = new Mock<IChromeValidator>();
        _loggerMock = new Mock<ILogger<CdpConnectionFactory>>();
        _options = new CdpOptions
        {
            EndpointUrl = "http://localhost:9222",
            ConnectionTimeoutSeconds = 30
        };
    }

    #region Success Scenarios

    [Fact]
    public async Task ConnectAsync_WhenChromeRunning_ReturnsConnected()
    {
        // Arrange
        var validationResult = ChromeValidationResult.Success(
            "Chrome/120.0.0.0",
            "/tmp/chrome-profile");

        _chromeValidatorMock
            .Setup(v => v.CheckPortAvailabilityAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        var mockPlaywright = CreateMockPlaywright(success: true);
        var factory = CreateFactory(mockPlaywright.Object);

        // Act
        var result = await factory.ConnectAsync();

        // Assert
        result.IsConnected.Should().BeTrue();
        result.Browser.Should().NotBeNull();
        result.BrowserVersion.Should().Be("120.0.0.0");
        result.UserDataDir.Should().Be("/tmp/chrome-profile");
        result.ErrorMessage.Should().BeNull();
        result.GuidanceMessage.Should().BeNull();
    }

    [Fact]
    public async Task ConnectAsync_WhenChromeRunningWithoutUserDataDir_ReturnsConnected()
    {
        // Arrange
        var validationResult = ChromeValidationResult.Success("Chrome/120.0.0.0", null);

        _chromeValidatorMock
            .Setup(v => v.CheckPortAvailabilityAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        var mockPlaywright = CreateMockPlaywright(success: true);
        var factory = CreateFactory(mockPlaywright.Object);

        // Act
        var result = await factory.ConnectAsync();

        // Assert
        result.IsConnected.Should().BeTrue();
        result.UserDataDir.Should().BeNull();
    }

    [Fact]
    public async Task ConnectAsync_WithCustomEndpoint_UsesCorrectUrl()
    {
        // Arrange
        _options.EndpointUrl = "http://custom-host:8080";

        var validationResult = ChromeValidationResult.Success("Chrome/120.0.0.0");

        _chromeValidatorMock
            .Setup(v => v.CheckPortAvailabilityAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        string? capturedEndpointUrl = null;
        var mockPlaywright = CreateMockPlaywright(success: true, captureUrl: url => capturedEndpointUrl = url);
        var factory = CreateFactory(mockPlaywright.Object);

        // Act
        var result = await factory.ConnectAsync();

        // Assert
        result.IsConnected.Should().BeTrue();
        capturedEndpointUrl.Should().Be("http://custom-host:8080");
    }

    [Fact]
    public async Task ConnectAsync_ValidatesSessionByCheckingBrowserVersion()
    {
        // Arrange
        var validationResult = ChromeValidationResult.Success("Chrome/120.0.0.0");

        _chromeValidatorMock
            .Setup(v => v.CheckPortAvailabilityAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        var mockPlaywright = CreateMockPlaywright(success: true, browserVersion: "120.0.6099.109");
        var factory = CreateFactory(mockPlaywright.Object);

        // Act
        var result = await factory.ConnectAsync();

        // Assert
        result.IsConnected.Should().BeTrue();
        result.BrowserVersion.Should().Be("120.0.6099.109");
    }

    #endregion

    #region Pre-flight Validation Failure Scenarios

    [Fact]
    public async Task ConnectAsync_WhenPreflightValidationFails_ReturnsFailure()
    {
        // Arrange
        var validationResult = ChromeValidationResult.Failure(
            "Connection refused",
            "Start Chrome with --remote-debugging-port=9222");

        _chromeValidatorMock
            .Setup(v => v.CheckPortAvailabilityAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        var mockPlaywright = CreateMockPlaywright(success: true);
        var factory = CreateFactory(mockPlaywright.Object);

        // Act
        var result = await factory.ConnectAsync();

        // Assert
        result.IsConnected.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Pre-flight validation failed");
        result.ErrorMessage.Should().Contain("Connection refused");
        result.GuidanceMessage.Should().Contain("--remote-debugging-port=9222");
    }

    [Fact]
    public async Task ConnectAsync_WhenChromeNotRunning_ReturnsFailure()
    {
        // Arrange
        var validationResult = ChromeValidationResult.Failure(
            "Connection refused - Chrome is not running or remote debugging is disabled",
            "Start Chrome with remote debugging enabled:\n\n" +
            "macOS: /Applications/Google\\ Chrome.app/Contents/MacOS/Google\\ Chrome --remote-debugging-port=9222");

        _chromeValidatorMock
            .Setup(v => v.CheckPortAvailabilityAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        var mockPlaywright = CreateMockPlaywright(success: true);
        var factory = CreateFactory(mockPlaywright.Object);

        // Act
        var result = await factory.ConnectAsync();

        // Assert
        result.IsConnected.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Pre-flight validation failed");
        result.GuidanceMessage.Should().Contain("macOS");
    }

    #endregion

    #region Timeout Scenarios

    [Fact]
    public async Task ConnectAsync_WhenTimeout_ReturnsFailure()
    {
        // Arrange
        var validationResult = ChromeValidationResult.Success("Chrome/120.0.0.0");

        _chromeValidatorMock
            .Setup(v => v.CheckPortAvailabilityAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        var mockPlaywright = CreateMockPlaywrightWithTimeout();
        var factory = CreateFactory(mockPlaywright.Object);

        // Act
        var result = await factory.ConnectAsync();

        // Assert
        result.IsConnected.Should().BeFalse();
        result.ErrorMessage.Should().Contain("timed out");
        result.GuidanceMessage.Should().NotBeNull();
    }

    [Fact]
    public async Task ConnectAsync_WhenTimeout_ContainsGuidance()
    {
        // Arrange
        var validationResult = ChromeValidationResult.Success("Chrome/120.0.0.0");

        _chromeValidatorMock
            .Setup(v => v.CheckPortAvailabilityAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        var mockPlaywright = CreateMockPlaywrightWithTimeout();
        var factory = CreateFactory(mockPlaywright.Object);

        // Act
        var result = await factory.ConnectAsync();

        // Assert
        result.GuidanceMessage.Should().Contain("--remote-debugging-port=9222");
        result.GuidanceMessage.Should().Contain("macOS");
        result.GuidanceMessage.Should().Contain("Linux");
        result.GuidanceMessage.Should().Contain("Windows");
    }

    #endregion

    #region Playwright Exception Scenarios

    [Fact]
    public async Task ConnectAsync_WhenPlaywrightConnectionRefused_ReturnsFailure()
    {
        // Arrange
        var validationResult = ChromeValidationResult.Success("Chrome/120.0.0.0");

        _chromeValidatorMock
            .Setup(v => v.CheckPortAvailabilityAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        var mockPlaywright = CreateMockPlaywrightWithException(
            new PlaywrightException("Connection refused"));

        var factory = CreateFactory(mockPlaywright.Object);

        // Act
        var result = await factory.ConnectAsync();

        // Assert
        result.IsConnected.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Connection refused");
        result.GuidanceMessage.Should().Contain("--remote-debugging-port=9222");
    }

    [Fact]
    public async Task ConnectAsync_WhenPlaywrightTimeout_ReturnsFailure()
    {
        // Arrange
        var validationResult = ChromeValidationResult.Success("Chrome/120.0.0.0");

        _chromeValidatorMock
            .Setup(v => v.CheckPortAvailabilityAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        var mockPlaywright = CreateMockPlaywrightWithException(
            new PlaywrightException("Timeout while connecting to browser"));

        var factory = CreateFactory(mockPlaywright.Object);

        // Act
        var result = await factory.ConnectAsync();

        // Assert
        result.IsConnected.Should().BeFalse();
        result.ErrorMessage.Should().Contain("timed out");
        result.GuidanceMessage.Should().NotBeNull();
    }

    [Fact]
    public async Task ConnectAsync_WhenPlaywrightInvalidEndpoint_ReturnsFailure()
    {
        // Arrange
        var validationResult = ChromeValidationResult.Success("Chrome/120.0.0.0");

        _chromeValidatorMock
            .Setup(v => v.CheckPortAvailabilityAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        var mockPlaywright = CreateMockPlaywrightWithException(
            new PlaywrightException("Invalid endpoint URL"));

        var factory = CreateFactory(mockPlaywright.Object);

        // Act
        var result = await factory.ConnectAsync();

        // Assert
        result.IsConnected.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid CDP endpoint URL");
        result.GuidanceMessage.Should().Contain("EndpointUrl");
    }

    [Fact]
    public async Task ConnectAsync_WhenPlaywrightUnauthorized_ReturnsFailure()
    {
        // Arrange
        var validationResult = ChromeValidationResult.Success("Chrome/120.0.0.0");

        _chromeValidatorMock
            .Setup(v => v.CheckPortAvailabilityAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        var mockPlaywright = CreateMockPlaywrightWithException(
            new PlaywrightException("Unauthorized access to DevTools"));

        var factory = CreateFactory(mockPlaywright.Object);

        // Act
        var result = await factory.ConnectAsync();

        // Assert
        result.IsConnected.Should().BeFalse();
        result.ErrorMessage.Should().Contain("denied");
        result.GuidanceMessage.Should().Contain("authentication");
    }

    [Fact]
    public async Task ConnectAsync_WhenPlaywrightDisconnected_ReturnsFailure()
    {
        // Arrange
        var validationResult = ChromeValidationResult.Success("Chrome/120.0.0.0");

        _chromeValidatorMock
            .Setup(v => v.CheckPortAvailabilityAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        var mockPlaywright = CreateMockPlaywrightWithException(
            new PlaywrightException("Target closed unexpectedly"));

        var factory = CreateFactory(mockPlaywright.Object);

        // Act
        var result = await factory.ConnectAsync();

        // Assert
        result.IsConnected.Should().BeFalse();
        result.ErrorMessage.Should().Contain("closed unexpectedly");
        result.GuidanceMessage.Should().NotBeNull();
    }

    [Fact]
    public async Task ConnectAsync_WhenPlaywrightGenericError_ReturnsFailure()
    {
        // Arrange
        var validationResult = ChromeValidationResult.Success("Chrome/120.0.0.0");

        _chromeValidatorMock
            .Setup(v => v.CheckPortAvailabilityAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        var mockPlaywright = CreateMockPlaywrightWithException(
            new PlaywrightException("Some unknown error"));

        var factory = CreateFactory(mockPlaywright.Object);

        // Act
        var result = await factory.ConnectAsync();

        // Assert
        result.IsConnected.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Playwright error");
        result.ErrorMessage.Should().Contain("Some unknown error");
        result.GuidanceMessage.Should().Contain("--remote-debugging-port=9222");
    }

    #endregion

    #region Cancellation Scenarios

    [Fact]
    public async Task ConnectAsync_WhenCancelled_ReturnsFailure()
    {
        // Arrange
        var validationResult = ChromeValidationResult.Success("Chrome/120.0.0.0");

        _chromeValidatorMock
            .Setup(v => v.CheckPortAvailabilityAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        var mockPlaywright = CreateMockPlaywrightWithCancellation();
        var factory = CreateFactory(mockPlaywright.Object);

        // Act
        var cts = new CancellationTokenSource();
        cts.Cancel();
        var result = await factory.ConnectAsync(cts.Token);

        // Assert
        result.IsConnected.Should().BeFalse();
        result.ErrorMessage.Should().Contain("cancelled");
    }

    #endregion

    #region Constructor Validation

    [Fact]
    public void Constructor_WithNullChromeValidator_ThrowsArgumentNullException()
    {
        // Arrange
        var options = Options.Create(_options);

        // Act & Assert
        var act = () => new CdpConnectionFactory(
            null!,
            _loggerMock.Object,
            options);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("chromeValidator");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var options = Options.Create(_options);

        // Act & Assert
        var act = () => new CdpConnectionFactory(
            _chromeValidatorMock.Object,
            null!,
            options);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new CdpConnectionFactory(
            _chromeValidatorMock.Object,
            _loggerMock.Object,
            null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("options");
    }

    #endregion

    #region Helper Methods

    private CdpConnectionFactory CreateFactory(IPlaywright playwright)
    {
        return new CdpConnectionFactory(
            _chromeValidatorMock.Object,
            _loggerMock.Object,
            Options.Create(_options),
            playwright);
    }

    private static Mock<IPlaywright> CreateMockPlaywright(
        bool success,
        string browserVersion = "120.0.0.0",
        Action<string>? captureUrl = null)
    {
        var mockBrowser = new Mock<IBrowser>();
        mockBrowser.SetupGet(b => b.Version).Returns(browserVersion);
        mockBrowser.SetupGet(b => b.Contexts).Returns(new List<IBrowserContext>());

        var mockChromium = new Mock<IBrowserType>();
        mockChromium
            .Setup(c => c.ConnectOverCDPAsync(
                It.IsAny<string>(),
                It.IsAny<BrowserTypeConnectOverCDPOptions?>()))
            .Callback<string, BrowserTypeConnectOverCDPOptions?>((url, _) => captureUrl?.Invoke(url))
            .ReturnsAsync(mockBrowser.Object);

        var mockPlaywright = new Mock<IPlaywright>();
        mockPlaywright.SetupGet(p => p.Chromium).Returns(mockChromium.Object);

        return mockPlaywright;
    }

    private static Mock<IPlaywright> CreateMockPlaywrightWithTimeout()
    {
        var mockChromium = new Mock<IBrowserType>();
        mockChromium
            .Setup(c => c.ConnectOverCDPAsync(
                It.IsAny<string>(),
                It.IsAny<BrowserTypeConnectOverCDPOptions?>()))
            .Returns(async () =>
            {
                await Task.Delay(5000); // Simulate a slow connection
                throw new TimeoutException("Connection timed out");
            });

        var mockPlaywright = new Mock<IPlaywright>();
        mockPlaywright.SetupGet(p => p.Chromium).Returns(mockChromium.Object);

        return mockPlaywright;
    }

    private static Mock<IPlaywright> CreateMockPlaywrightWithException(PlaywrightException exception)
    {
        var mockChromium = new Mock<IBrowserType>();
        mockChromium
            .Setup(c => c.ConnectOverCDPAsync(
                It.IsAny<string>(),
                It.IsAny<BrowserTypeConnectOverCDPOptions?>()))
            .ThrowsAsync(exception);

        var mockPlaywright = new Mock<IPlaywright>();
        mockPlaywright.SetupGet(p => p.Chromium).Returns(mockChromium.Object);

        return mockPlaywright;
    }

    private static Mock<IPlaywright> CreateMockPlaywrightWithCancellation()
    {
        var mockChromium = new Mock<IBrowserType>();
        mockChromium
            .Setup(c => c.ConnectOverCDPAsync(
                It.IsAny<string>(),
                It.IsAny<BrowserTypeConnectOverCDPOptions?>()))
            .ThrowsAsync(new OperationCanceledException());

        var mockPlaywright = new Mock<IPlaywright>();
        mockPlaywright.SetupGet(p => p.Chromium).Returns(mockChromium.Object);

        return mockPlaywright;
    }

    #endregion
}
