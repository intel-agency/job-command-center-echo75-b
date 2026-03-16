#nullable enable

using FluentAssertions;
using JobCommandCenter.Harvester.Models;
using JobCommandCenter.Harvester.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Moq;
using Xunit;

namespace JobCommandCenter.UnitTests.Harvester;

/// <summary>
/// Unit tests for TabManager tab management functionality.
/// </summary>
public class TabManagerTests
{
    private readonly Mock<IBrowser> _browserMock;
    private readonly Mock<ILogger<TabManager>> _loggerMock;

    public TabManagerTests()
    {
        _browserMock = new Mock<IBrowser>();
        _loggerMock = new Mock<ILogger<TabManager>>();
    }

    #region GetTabsAsync Tests

    [Fact]
    public async Task GetTabsAsync_ReturnsAllOpenTabs()
    {
        // Arrange
        var mockPage1 = CreateMockPage("page1", "https://linkedin.com/jobs/search", "LinkedIn Jobs");
        var mockPage2 = CreateMockPage("page2", "https://google.com", "Google");
        var mockContext = CreateMockContext(new[] { mockPage1.Object, mockPage2.Object });

        _browserMock.SetupGet(b => b.Contexts).Returns(new List<IBrowserContext> { mockContext.Object });

        var tabManager = CreateTabManager();

        // Act
        var result = await tabManager.GetTabsAsync();

        // Assert
        result.Should().HaveCount(2);
        result[0].Url.Should().Be("https://linkedin.com/jobs/search");
        result[0].Title.Should().Be("LinkedIn Jobs");
        result[0].IsLinkedIn.Should().BeTrue();
        result[0].IsJobPage.Should().BeTrue();
        result[1].Url.Should().Be("https://google.com");
        result[1].IsLinkedIn.Should().BeFalse();
    }

    [Fact]
    public async Task GetTabsAsync_WhenNoTabs_ReturnsEmptyList()
    {
        // Arrange
        var mockContext = CreateMockContext(Array.Empty<IPage>());
        _browserMock.SetupGet(b => b.Contexts).Returns(new List<IBrowserContext> { mockContext.Object });

        var tabManager = CreateTabManager();

        // Act
        var result = await tabManager.GetTabsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTabsAsync_WithMultipleContexts_ReturnsTabsFromAllContexts()
    {
        // Arrange
        var mockPage1 = CreateMockPage("page1", "https://site1.com", "Site 1");
        var mockPage2 = CreateMockPage("page2", "https://site2.com", "Site 2");
        var mockContext1 = CreateMockContext(new[] { mockPage1.Object });
        var mockContext2 = CreateMockContext(new[] { mockPage2.Object });

        _browserMock.SetupGet(b => b.Contexts).Returns(new List<IBrowserContext> { mockContext1.Object, mockContext2.Object });

        var tabManager = CreateTabManager();

        // Act
        var result = await tabManager.GetTabsAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region FindLinkedInTabAsync Tests

    [Fact]
    public async Task FindLinkedInTabAsync_WhenLinkedInTabExists_ReturnsTab()
    {
        // Arrange
        var mockPage1 = CreateMockPage("page1", "https://google.com", "Google");
        var mockPage2 = CreateMockPage("page2", "https://linkedin.com/jobs/view/123", "Job Title - LinkedIn");
        var mockContext = CreateMockContext(new[] { mockPage1.Object, mockPage2.Object });

        _browserMock.SetupGet(b => b.Contexts).Returns(new List<IBrowserContext> { mockContext.Object });

        var tabManager = CreateTabManager();

        // Act
        var result = await tabManager.FindLinkedInTabAsync();

        // Assert
        result.Should().NotBeNull();
        result!.Url.Should().Contain("linkedin.com/jobs");
        result.IsJobPage.Should().BeTrue();
    }

    [Fact]
    public async Task FindLinkedInTabAsync_WhenNoLinkedInTab_ReturnsNull()
    {
        // Arrange
        var mockPage1 = CreateMockPage("page1", "https://google.com", "Google");
        var mockPage2 = CreateMockPage("page2", "https://github.com", "GitHub");
        var mockContext = CreateMockContext(new[] { mockPage1.Object, mockPage2.Object });

        _browserMock.SetupGet(b => b.Contexts).Returns(new List<IBrowserContext> { mockContext.Object });

        var tabManager = CreateTabManager();

        // Act
        var result = await tabManager.FindLinkedInTabAsync();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task FindLinkedInTabAsync_WhenOnlyLinkedInNonJobPage_ReturnsNull()
    {
        // Arrange
        var mockPage = CreateMockPage("page1", "https://linkedin.com/in/someprofile", "Profile");
        var mockContext = CreateMockContext(new[] { mockPage.Object });

        _browserMock.SetupGet(b => b.Contexts).Returns(new List<IBrowserContext> { mockContext.Object });

        var tabManager = CreateTabManager();

        // Act
        var result = await tabManager.FindLinkedInTabAsync();

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateNewTabAsync Tests

    [Fact]
    public async Task CreateNewTabAsync_CreatesTabWithCorrectUrl()
    {
        // Arrange
        var mockContext = new Mock<IBrowserContext>();
        var mockNewPage = CreateMockPage("newpage", "https://linkedin.com/jobs", "LinkedIn Jobs");

        mockContext.Setup(c => c.NewPageAsync())
            .ReturnsAsync(mockNewPage.Object);
        mockContext.SetupGet(c => c.Pages).Returns(new List<IPage> { mockNewPage.Object });

        _browserMock.SetupGet(b => b.Contexts).Returns(new List<IBrowserContext> { mockContext.Object });

        var tabManager = CreateTabManager();

        // Act
        var result = await tabManager.CreateNewTabAsync("https://linkedin.com/jobs");

        // Assert
        result.Should().NotBeNull();
        result.Url.Should().Be("https://linkedin.com/jobs");
        mockContext.Verify(c => c.NewPageAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateNewTabAsync_WhenNoContext_ThrowsInvalidOperationException()
    {
        // Arrange
        _browserMock.SetupGet(b => b.Contexts).Returns(new List<IBrowserContext>());

        var tabManager = CreateTabManager();

        // Act & Assert
        var act = () => tabManager.CreateNewTabAsync("https://linkedin.com/jobs");
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No browser context available*");
    }

    [Fact]
    public async Task CreateNewTabAsync_WithEmptyUrl_ThrowsArgumentException()
    {
        // Arrange
        var tabManager = CreateTabManager();

        // Act & Assert
        var act = () => tabManager.CreateNewTabAsync("");
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CreateNewTabAsync_WithNullUrl_ThrowsArgumentException()
    {
        // Arrange
        var tabManager = CreateTabManager();

        // Act & Assert
        var act = () => tabManager.CreateNewTabAsync(null!);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    #endregion

    #region CloseTabAsync Tests

    [Fact]
    public async Task CloseTabAsync_ClosesExistingTab()
    {
        // Arrange
        var mockPage = CreateMockPage("123", "https://linkedin.com/jobs", "LinkedIn Jobs");
        var otherPage = CreateMockPage("456", "about:blank", "");
        var mockContext = CreateMockContext(new[] { mockPage.Object, otherPage.Object });

        _browserMock.SetupGet(b => b.Contexts).Returns(new List<IBrowserContext> { mockContext.Object });

        var tabManager = CreateTabManager();

        // The tab ID is the page's hash code as a string
        var tabId = mockPage.Object.GetHashCode().ToString();

        // Act
        var result = await tabManager.CloseTabAsync(tabId);

        // Assert
        result.Should().BeTrue();
        mockPage.Verify(p => p.CloseAsync(It.IsAny<PageCloseOptions?>()), Times.Once);
    }

    [Fact]
    public async Task CloseTabAsync_WhenLastTab_ReturnsFalse()
    {
        // Arrange
        var mockPage = CreateMockPage("123", "https://linkedin.com/jobs", "LinkedIn Jobs");
        var mockContext = CreateMockContext(new[] { mockPage.Object });

        _browserMock.SetupGet(b => b.Contexts).Returns(new List<IBrowserContext> { mockContext.Object });

        var tabManager = CreateTabManager();

        // Act
        var result = await tabManager.CloseTabAsync("123");

        // Assert
        result.Should().BeFalse();
        mockPage.Verify(p => p.CloseAsync(It.IsAny<PageCloseOptions?>()), Times.Never);
    }

    [Fact]
    public async Task CloseTabAsync_WhenTabNotFound_ReturnsFalse()
    {
        // Arrange
        var mockPage = CreateMockPage("123", "https://linkedin.com/jobs", "LinkedIn Jobs");
        var mockContext = CreateMockContext(new[] { mockPage.Object });

        _browserMock.SetupGet(b => b.Contexts).Returns(new List<IBrowserContext> { mockContext.Object });

        var tabManager = CreateTabManager();

        // Act
        var result = await tabManager.CloseTabAsync("nonexistent");

        // Assert
        result.Should().BeFalse();
        mockPage.Verify(p => p.CloseAsync(It.IsAny<PageCloseOptions?>()), Times.Never);
    }

    [Fact]
    public async Task CloseTabAsync_WithEmptyTabId_ThrowsArgumentException()
    {
        // Arrange
        var tabManager = CreateTabManager();

        // Act & Assert
        var act = () => tabManager.CloseTabAsync("");
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CloseTabAsync_WithNullTabId_ThrowsArgumentException()
    {
        // Arrange
        var tabManager = CreateTabManager();

        // Act & Assert
        var act = () => tabManager.CloseTabAsync(null!);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CloseTabAsync_WhenPlaywrightException_ReturnsFalse()
    {
        // Arrange
        var mockPage = CreateMockPage("123", "https://linkedin.com/jobs", "LinkedIn Jobs");
        var mockContext = CreateMockContext(new[] { mockPage.Object, CreateMockPage("456", "about:blank", "").Object });

        mockPage.Setup(p => p.CloseAsync(It.IsAny<PageCloseOptions?>()))
            .ThrowsAsync(new PlaywrightException("Tab already closed"));

        _browserMock.SetupGet(b => b.Contexts).Returns(new List<IBrowserContext> { mockContext.Object });

        var tabManager = CreateTabManager();

        // Act
        var result = await tabManager.CloseTabAsync("123");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Constructor Validation Tests

    [Fact]
    public void Constructor_WithNullBrowser_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new TabManager(null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("browser");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new TabManager(_browserMock.Object, null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task GetTabsAsync_WhenCancelled_ThrowsOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _browserMock.SetupGet(b => b.Contexts).Returns(new List<IBrowserContext>());

        var tabManager = CreateTabManager();

        // Act & Assert
        var act = () => tabManager.GetTabsAsync(cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task FindLinkedInTabAsync_WhenCancelled_ThrowsOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _browserMock.SetupGet(b => b.Contexts).Returns(new List<IBrowserContext>());

        var tabManager = CreateTabManager();

        // Act & Assert
        var act = () => tabManager.FindLinkedInTabAsync(cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region Helper Methods

    private TabManager CreateTabManager()
    {
        return new TabManager(_browserMock.Object, _loggerMock.Object);
    }

    private static Mock<IPage> CreateMockPage(string id, string url, string title)
    {
        var mockPage = new Mock<IPage>();
        mockPage.SetupGet(p => p.Url).Returns(url);
        mockPage.Setup(p => p.TitleAsync()).ReturnsAsync(title);
        mockPage.Setup(p => p.GetHashCode()).Returns(id.GetHashCode());
        mockPage.Setup(p => p.CloseAsync(It.IsAny<PageCloseOptions?>())).Returns(Task.CompletedTask);
        return mockPage;
    }

    private static Mock<IBrowserContext> CreateMockContext(IReadOnlyList<IPage> pages)
    {
        var mockContext = new Mock<IBrowserContext>();
        mockContext.SetupGet(c => c.Pages).Returns(pages);
        return mockContext;
    }

    #endregion
}
