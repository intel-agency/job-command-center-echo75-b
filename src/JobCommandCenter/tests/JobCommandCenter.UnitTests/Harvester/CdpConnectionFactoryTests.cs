#nullable enable

using FluentAssertions;
using JobCommandCenter.Harvester.Configuration;
using JobCommandCenter.Harvester.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Moq;
using Xunit;

namespace JobCommandCenter.UnitTests.Harvester;

/// <summary>
/// Unit tests for CdpConnectionFactory CDP connection functionality.
/// </summary>
public class CdpConnectionFactoryTests : IAsyncLifetime
{
    private readonly Mock<IPlaywright> _playwrightMock;
    private readonly Mock<IBrowserType> _browserTypeMock;
    private readonly Mock<IBrowser> _browserMock;
    private readonly Mock<IBrowserContext> _contextMock;
    private readonly Mock<IPage> _pageMock;
    private readonly Mock<ILogger<CdpConnectionFactory>> _loggerMock;
    private readonly ChromeCdpOptions _options;

    public CdpConnectionFactoryTests()
    {
        _playwrightMock = new Mock<IPlaywright>();
        _browserTypeMock = new Mock<IBrowserType>();
        _browserMock = new Mock<IBrowser>();
        _contextMock = new Mock<IBrowserContext>();
        _pageMock = new Mock<IPage>();
        _loggerMock = new Mock<ILogger<CdpConnectionFactory>>();
        _options = new ChromeCdpOptions
        {
            Port = 9222,
            ConnectionTimeoutSeconds = 30
        };
    }

    public Task InitializeAsync()
    {
        // Setup playwright chromium browser type
        _playwrightMock.SetupGet(p => p.Chromium).Returns(_browserTypeMock.Object);
        
        // Setup browser with version and contexts
        _browserMock.SetupGet(b => b.Version).Returns("120.0.6099.109");
        _browserMock.SetupGet(b => b.Contexts).Returns(new List<IBrowserContext> { _contextMock.Object });
        
        // Setup context with pages
        _contextMock.SetupGet(c => c.Pages).Returns(new List<IPage> { _pageMock.Object });

        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    #region Success Scenarios

    [Fact]
    public async Task ConnectAsync_WhenChromeRunning_ReturnsConnectedBrowser()
    {
        // Arrange
        _browserTypeMock
            .Setup(bt => bt.ConnectOverCDPAsync(It.IsAny<string>(), It.IsAny<BrowserTypeConnectOverCDPOptions?>()))
            .ReturnsAsync(_browserMock.Object);

        var factory = CreateFactory();

        // Act
        var browser = await factory.ConnectAsync();

        // Assert
        browser.Should().NotBeNull();
        browser.Should().BeSameAs(_browserMock.Object);
        _browserTypeMock.Verify(bt => bt.ConnectOverCDPAsync("http://localhost:9222", null), Times.Once);
    }

    [Fact]
    public async Task ConnectAsync_WhenChromeRunning_LogsConnectionAttempt()
    {
        // Arrange
        _browserTypeMock
            .Setup(bt => bt.ConnectOverCDPAsync(It.IsAny<string>(), It.IsAny<BrowserTypeConnectOverCDPOptions?>()))
            .ReturnsAsync(_browserMock.Object);

        var factory = CreateFactory();

        // Act
        await factory.ConnectAsync();

        // Assert
        _loggerMock.VerifyLog(
            LogLevel.Information,
            "Connecting to Chrome CDP at*",
            Times.Once());
    }

    [Fact]
    public async Task ConnectAsync_WhenChromeRunning_LogsSuccessfulConnection()
    {
        // Arrange
        _browserTypeMock
            .Setup(bt => bt.ConnectOverCDPAsync(It.IsAny<string>(), It.IsAny<BrowserTypeConnectOverCDPOptions?>()))
            .ReturnsAsync(_browserMock.Object);

        var factory = CreateFactory();

        // Act
        await factory.ConnectAsync();

        // Assert
        _loggerMock.VerifyLog(
            LogLevel.Information,
            "Successfully connected to Chrome*",
            Times.Once());
    }

    [Fact]
    public async Task ConnectAsync_WithCustomPort_UsesCorrectEndpoint()
    {
        // Arrange
        _options.Port = 8080;
        _browserTypeMock
            .Setup(bt => bt.ConnectOverCDPAsync(It.IsAny<string>(), It.IsAny<BrowserTypeConnectOverCDPOptions?>()))
            .ReturnsAsync(_browserMock.Object);

        var factory = CreateFactory();

        // Act
        await factory.ConnectAsync();

        // Assert
        _browserTypeMock.Verify(bt => bt.ConnectOverCDPAsync("http://localhost:8080", null), Times.Once);
    }

    [Fact]
    public async Task ConnectAsync_ValidatesBrowserVersionAfterConnection()
    {
        // Arrange
        _browserMock.SetupGet(b => b.Version).Returns("120.0.6099.109");
        _browserTypeMock
            .Setup(bt => bt.ConnectOverCDPAsync(It.IsAny<string>(), It.IsAny<BrowserTypeConnectOverCDPOptions?>()))
            .ReturnsAsync(_browserMock.Object);

        var factory = CreateFactory();

        // Act
        var browser = await factory.ConnectAsync();

        // Assert
        browser.Version.Should().Be("120.0.6099.109");
    }

    [Fact]
    public async Task ConnectAsync_ValidatesBrowserContextsAfterConnection()
    {
        // Arrange
        _browserMock.SetupGet(b => b.Contexts).Returns(new List<IBrowserContext> { _contextMock.Object });
        _browserTypeMock
            .Setup(bt => bt.ConnectOverCDPAsync(It.IsAny<string>(), It.IsAny<BrowserTypeConnectOverCDPOptions?>()))
            .ReturnsAsync(_browserMock.Object);

        var factory = CreateFactory();

        // Act
        var browser = await factory.ConnectAsync();

        // Assert
        browser.Contexts.Should().HaveCount(1);
    }

    #endregion

    #region Connection Timeout Scenarios

    [Fact]
    public async Task ConnectAsync_WhenTimeout_ThrowsTimeoutException()
    {
        // Arrange
        var shortTimeoutOptions = new ChromeCdpOptions
        {
            Port = 9222,
            ConnectionTimeoutSeconds = 1
        };

        _browserTypeMock
            .Setup(bt => bt.ConnectOverCDPAsync(It.IsAny<string>(), It.IsAny<BrowserTypeConnectOverCDPOptions?>()))
            .Returns(async () =>
            {
                await Task.Delay(5000); // Simulate long connection
                return _browserMock.Object;
            });

        var factory = CreateFactory(shortTimeoutOptions);

        // Act
        var act = () => factory.ConnectAsync();

        // Assert
        await act.Should().ThrowAsync<TimeoutException>()
            .WithMessage("*timed out*");
    }

    [Fact]
    public async Task ConnectAsync_WhenTimeout_LogsError()
    {
        // Arrange
        var shortTimeoutOptions = new ChromeCdpOptions
        {
            Port = 9222,
            ConnectionTimeoutSeconds = 1
        };

        _browserTypeMock
            .Setup(bt => bt.ConnectOverCDPAsync(It.IsAny<string>(), It.IsAny<BrowserTypeConnectOverCDPOptions?>()))
            .Returns(async () =>
            {
                await Task.Delay(5000); // Simulate long connection
                return _browserMock.Object;
            });

        var factory = CreateFactory(shortTimeoutOptions);

        // Act
        try
        {
            await factory.ConnectAsync();
        }
        catch (TimeoutException)
        {
            // Expected
        }

        // Assert
        _loggerMock.VerifyLog(
            LogLevel.Error,
            "*timed out*",
            Times.Once());
    }

    [Fact]
    public async Task ConnectAsync_WhenTimeout_ContainsGuidanceMessage()
    {
        // Arrange
        var shortTimeoutOptions = new ChromeCdpOptions
        {
            Port = 9222,
            ConnectionTimeoutSeconds = 1
        };

        _browserTypeMock
            .Setup(bt => bt.ConnectOverCDPAsync(It.IsAny<string>(), It.IsAny<BrowserTypeConnectOverCDPOptions?>()))
            .Returns(async () =>
            {
                await Task.Delay(5000); // Simulate long connection
                return _browserMock.Object;
            });

        var factory = CreateFactory(shortTimeoutOptions);

        // Act
        var act = () => factory.ConnectAsync();

        // Assert
        var exception = await act.Should().ThrowAsync<TimeoutException>();
        exception.Which.Message.Should().Contain("--remote-debugging-port=9222");
        exception.Which.Message.Should().Contain("macOS");
        exception.Which.Message.Should().Contain("Linux");
        exception.Which.Message.Should().Contain("Windows");
    }

    #endregion

    #region Connection Refused Scenarios

    [Fact]
    public async Task ConnectAsync_WhenConnectionRefused_ThrowsInvalidOperationException()
    {
        // Arrange
        _browserTypeMock
            .Setup(bt => bt.ConnectOverCDPAsync(It.IsAny<string>(), It.IsAny<BrowserTypeConnectOverCDPOptions?>()))
            .ThrowsAsync(new PlaywrightException("ECONNREFUSED: Connection refused"));

        var factory = CreateFactory();

        // Act
        var act = () => factory.ConnectAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Connection refused*");
    }

    [Fact]
    public async Task ConnectAsync_WhenConnectionRefused_LogsError()
    {
        // Arrange
        _browserTypeMock
            .Setup(bt => bt.ConnectOverCDPAsync(It.IsAny<string>(), It.IsAny<BrowserTypeConnectOverCDPOptions?>()))
            .ThrowsAsync(new PlaywrightException("ECONNREFUSED: Connection refused"));

        var factory = CreateFactory();

        // Act
        try
        {
            await factory.ConnectAsync();
        }
        catch (InvalidOperationException)
        {
            // Expected
        }

        // Assert
        _loggerMock.VerifyLog(
            LogLevel.Error,
            "*connection refused*",
            Times.Once());
    }

    [Fact]
    public async Task ConnectAsync_WhenConnectionRefused_ContainsGuidanceMessage()
    {
        // Arrange
        _browserTypeMock
            .Setup(bt => bt.ConnectOverCDPAsync(It.IsAny<string>(), It.IsAny<BrowserTypeConnectOverCDPOptions?>()))
            .ThrowsAsync(new PlaywrightException("ECONNREFUSED: Connection refused"));

        var factory = CreateFactory();

        // Act
        var act = () => factory.ConnectAsync();

        // Assert
        var exception = await act.Should().ThrowAsync<InvalidOperationException>();
        exception.Which.Message.Should().Contain("--remote-debugging-port=9222");
        exception.Which.Message.Should().Contain("macOS");
        exception.Which.Message.Should().Contain("Linux");
        exception.Which.Message.Should().Contain("Windows");
    }

    #endregion

    #region Invalid Endpoint Scenarios

    [Fact]
    public async Task ConnectAsync_WhenInvalidUrl_ThrowsArgumentException()
    {
        // Arrange
        _browserTypeMock
            .Setup(bt => bt.ConnectOverCDPAsync(It.IsAny<string>(), It.IsAny<BrowserTypeConnectOverCDPOptions?>()))
            .ThrowsAsync(new PlaywrightException("Invalid URL"));

        var factory = CreateFactory();

        // Act
        var act = () => factory.ConnectAsync();

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Invalid CDP endpoint URL*");
    }

    [Fact]
    public async Task ConnectAsync_WhenInvalidEndpoint_ThrowsArgumentException()
    {
        // Arrange
        _browserTypeMock
            .Setup(bt => bt.ConnectOverCDPAsync(It.IsAny<string>(), It.IsAny<BrowserTypeConnectOverCDPOptions?>()))
            .ThrowsAsync(new PlaywrightException("Invalid endpoint"));

        var factory = CreateFactory();

        // Act
        var act = () => factory.ConnectAsync();

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Invalid CDP endpoint URL*");
    }

    [Fact]
    public async Task ConnectAsync_WhenInvalidUrl_LogsError()
    {
        // Arrange
        _browserTypeMock
            .Setup(bt => bt.ConnectOverCDPAsync(It.IsAny<string>(), It.IsAny<BrowserTypeConnectOverCDPOptions?>()))
            .ThrowsAsync(new PlaywrightException("Invalid URL"));

        var factory = CreateFactory();

        // Act
        try
        {
            await factory.ConnectAsync();
        }
        catch (ArgumentException)
        {
            // Expected
        }

        // Assert
        _loggerMock.VerifyLog(
            LogLevel.Error,
            "*Invalid CDP endpoint URL*",
            Times.Once());
    }

    #endregion

    #region General Playwright Exception Scenarios

    [Fact]
    public async Task ConnectAsync_WhenGenericPlaywrightError_ThrowsInvalidOperationException()
    {
        // Arrange
        _browserTypeMock
            .Setup(bt => bt.ConnectOverCDPAsync(It.IsAny<string>(), It.IsAny<BrowserTypeConnectOverCDPOptions?>()))
            .ThrowsAsync(new PlaywrightException("Some other Playwright error"));

        var factory = CreateFactory();

        // Act
        var act = () => factory.ConnectAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Failed to connect to Chrome*");
    }

    [Fact]
    public async Task ConnectAsync_WhenGenericPlaywrightError_LogsError()
    {
        // Arrange
        _browserTypeMock
            .Setup(bt => bt.ConnectOverCDPAsync(It.IsAny<string>(), It.IsAny<BrowserTypeConnectOverCDPOptions?>()))
            .ThrowsAsync(new PlaywrightException("Some other Playwright error"));

        var factory = CreateFactory();

        // Act
        try
        {
            await factory.ConnectAsync();
        }
        catch (InvalidOperationException)
        {
            // Expected
        }

        // Assert
        _loggerMock.VerifyLog(
            LogLevel.Error,
            "*Failed to connect to Chrome*",
            Times.Once());
    }

    #endregion

    #region Cancellation Scenarios

    [Fact]
    public async Task ConnectAsync_WhenCancelled_ThrowsOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        
        _browserTypeMock
            .Setup(bt => bt.ConnectOverCDPAsync(It.IsAny<string>(), It.IsAny<BrowserTypeConnectOverCDPOptions?>()))
            .Returns(async () =>
            {
                await Task.Delay(5000, cts.Token); // Will be cancelled
                return _browserMock.Object;
            });

        var factory = CreateFactory();

        // Act
        var act = async () =>
        {
            cts.CancelAfter(100);
            await factory.ConnectAsync(cts.Token);
        };

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region Constructor Validation

    [Fact]
    public void Constructor_WhenPlaywrightIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new CdpConnectionFactory(null!, _options, _loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("playwright");
    }

    [Fact]
    public void Constructor_WhenOptionsIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new CdpConnectionFactory(_playwrightMock.Object, null!, _loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("options");
    }

    [Fact]
    public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new CdpConnectionFactory(_playwrightMock.Object, _options, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    #endregion

    #region Helper Methods

    private CdpConnectionFactory CreateFactory(ChromeCdpOptions? options = null)
    {
        return new CdpConnectionFactory(
            _playwrightMock.Object,
            options ?? _options,
            _loggerMock.Object);
    }

    #endregion
}

/// <summary>
/// Extension methods for verifying logger calls.
/// </summary>
internal static class LoggerVerifyExtensions
{
    public static void VerifyLog(
        this Mock<ILogger<CdpConnectionFactory>> loggerMock,
        LogLevel logLevel,
        string messagePattern,
        Times times)
    {
        loggerMock.Verify(
            x => x.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(messagePattern.Replace("*", ""))),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
    }
}
