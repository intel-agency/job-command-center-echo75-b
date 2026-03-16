#nullable enable

using FluentAssertions;
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
    private readonly Mock<ILogger<TabManager>> _loggerMock;
    private readonly TabManager _tabManager;

    public TabManagerTests()
    {
        _loggerMock = new Mock<ILogger<TabManager>>();
        _tabManager = new TabManager(_loggerMock.Object);
    }

    #region Constructor Validation

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new TabManager(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    #endregion

    #region GetTabsAsync Tests

    [Fact]
    public async Task GetTabsAsync_ReturnsAllPagesFromAllContexts()
    {
        // Arrange
        var mockBrowser = new Mock<IBrowser>();
        
        var context1Pages = new List<IPage>
        {
            CreateMockPage("https://google.com", "Google").Object,
            CreateMockPage("https://linkedin.com/jobs", "LinkedIn Jobs").Object
        };
        
        var context2Pages = new List<IPage>
        {
            CreateMockPage("https://github.com", "GitHub").Object
        };

        var contexts = new List<IBrowserContext>
        {
            CreateMockContext(context1Pages).Object,
            CreateMockContext(context2Pages).Object
        };

        mockBrowser.SetupGet(b => b.Contexts).Returns(contexts);

        // Act
        var result = await _tabManager.GetTabsAsync(mockBrowser.Object);

        // Assert
        result.Should().HaveCount(3);
        result.Select(t => t.Url).Should().Contain([
            "https://google.com",
            "https://linkedin.com/jobs",
            "https://github.com"
        ]);
    }

    [Fact]
    public async Task GetTabsAsync_WithNoContexts_ReturnsEmptyList()
    {
        // Arrange
        var mockBrowser = new Mock<IBrowser>();
        mockBrowser.SetupGet(b => b.Contexts).Returns(new List<IBrowserContext>());

        // Act
        var result = await _tabManager.GetTabsAsync(mockBrowser.Object);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTabsAsync_WithContextWithNoPages_ReturnsEmptyList()
    {
        // Arrange
        var mockBrowser = new Mock<IBrowser>();
        var contexts = new List<IBrowserContext>
        {
            CreateMockContext(new List<IPage>()).Object
        };
        mockBrowser.SetupGet(b => b.Contexts).Returns(contexts);

        // Act
        var result = await _tabManager.GetTabsAsync(mockBrowser.Object);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTabsAsync_WithNullBrowser_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = async () => await _tabManager.GetTabsAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region FindLinkedInTabAsync Tests

    [Fact]
    public async Task FindLinkedInTabAsync_FindsLinkedInPageByUrl()
    {
        // Arrange
        var mockBrowser = new Mock<IBrowser>();
        
        var pages = new List<IPage>
        {
            CreateMockPage("https://google.com", "Google").Object,
            CreateMockPage("https://linkedin.com/jobs", "LinkedIn Jobs").Object,
            CreateMockPage("https://github.com", "GitHub").Object
        };

        var contexts = new List<IBrowserContext>
        {
            CreateMockContext(pages).Object
        };

        mockBrowser.SetupGet(b => b.Contexts).Returns(contexts);

        // Act
        var result = await _tabManager.FindLinkedInTabAsync(mockBrowser.Object);

        // Assert
        result.Should().NotBeNull();
        result!.Url.Should().Be("https://linkedin.com/jobs");
        result.IsLinkedIn.Should().BeTrue();
        result.Title.Should().Be("LinkedIn Jobs");
    }

    [Fact]
    public async Task FindLinkedInTabAsync_ReturnsNullWhenNoLinkedInPage()
    {
        // Arrange
        var mockBrowser = new Mock<IBrowser>();
        
        var pages = new List<IPage>
        {
            CreateMockPage("https://google.com", "Google").Object,
            CreateMockPage("https://github.com", "GitHub").Object
        };

        var contexts = new List<IBrowserContext>
        {
            CreateMockContext(pages).Object
        };

        mockBrowser.SetupGet(b => b.Contexts).Returns(contexts);

        // Act
        var result = await _tabManager.FindLinkedInTabAsync(mockBrowser.Object);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task FindLinkedInTabAsync_DetectsWwwLinkedIn()
    {
        // Arrange
        var mockBrowser = new Mock<IBrowser>();
        
        var pages = new List<IPage>
        {
            CreateMockPage("https://www.linkedin.com/feed", "LinkedIn Feed").Object
        };

        var contexts = new List<IBrowserContext>
        {
            CreateMockContext(pages).Object
        };

        mockBrowser.SetupGet(b => b.Contexts).Returns(contexts);

        // Act
        var result = await _tabManager.FindLinkedInTabAsync(mockBrowser.Object);

        // Assert
        result.Should().NotBeNull();
        result!.IsLinkedIn.Should().BeTrue();
    }

    [Fact]
    public async Task FindLinkedInTabAsync_DetectsSubdomainLinkedIn()
    {
        // Arrange
        var mockBrowser = new Mock<IBrowser>();
        
        var pages = new List<IPage>
        {
            CreateMockPage("https://app.linkedin.com/jobs", "LinkedIn App").Object
        };

        var contexts = new List<IBrowserContext>
        {
            CreateMockContext(pages).Object
        };

        mockBrowser.SetupGet(b => b.Contexts).Returns(contexts);

        // Act
        var result = await _tabManager.FindLinkedInTabAsync(mockBrowser.Object);

        // Assert
        result.Should().NotBeNull();
        result!.IsLinkedIn.Should().BeTrue();
    }

    [Fact]
    public async Task FindLinkedInTabAsync_DoesNotDetectFakeLinkedIn()
    {
        // Arrange
        var mockBrowser = new Mock<IBrowser>();
        
        var pages = new List<IPage>
        {
            CreateMockPage("https://linkedin.malicious.com", "Fake LinkedIn").Object
        };

        var contexts = new List<IBrowserContext>
        {
            CreateMockContext(pages).Object
        };

        mockBrowser.SetupGet(b => b.Contexts).Returns(contexts);

        // Act
        var result = await _tabManager.FindLinkedInTabAsync(mockBrowser.Object);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task FindLinkedInTabAsync_WithNullBrowser_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = async () => await _tabManager.FindLinkedInTabAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region CreateNewTabAsync Tests

    [Fact]
    public async Task CreateNewTabAsync_CreatesPageAndNavigates()
    {
        // Arrange
        var mockPage = CreateMockPage("https://www.linkedin.com/jobs/", "LinkedIn Jobs");
        string? navigatedUrl = null;

        mockPage
            .Setup(p => p.GotoAsync(
                It.IsAny<string>(),
                It.IsAny<PageGotoOptions?>()))
            .Callback<string, PageGotoOptions?>((url, _) => navigatedUrl = url)
            .ReturnsAsync(new Mock<IResponse>().Object);

        var mockContext = new Mock<IBrowserContext>();
        mockContext
            .Setup(c => c.NewPageAsync())
            .ReturnsAsync(mockPage.Object);
        mockContext.SetupGet(c => c.Pages).Returns(new List<IPage> { mockPage.Object });

        var mockBrowser = new Mock<IBrowser>();
        mockBrowser.SetupGet(b => b.Contexts).Returns(new List<IBrowserContext> { mockContext.Object });

        // Act
        var result = await _tabManager.CreateNewTabAsync(mockBrowser.Object);

        // Assert
        result.Should().NotBeNull();
        result.Page.Should().Be(mockPage.Object);
        navigatedUrl.Should().Be(LinkedInUrls.JobsPage);
    }

    [Fact]
    public async Task CreateNewTabAsync_WithCustomUrl_NavigatesToCustomUrl()
    {
        // Arrange
        var customUrl = "https://www.linkedin.com/jobs/search/?keywords=dotnet";
        var mockPage = CreateMockPage(customUrl, "Job Search");
        string? navigatedUrl = null;

        mockPage
            .Setup(p => p.GotoAsync(
                It.IsAny<string>(),
                It.IsAny<PageGotoOptions?>()))
            .Callback<string, PageGotoOptions?>((url, _) => navigatedUrl = url)
            .ReturnsAsync(new Mock<IResponse>().Object);

        var mockContext = new Mock<IBrowserContext>();
        mockContext
            .Setup(c => c.NewPageAsync())
            .ReturnsAsync(mockPage.Object);
        mockContext.SetupGet(c => c.Pages).Returns(new List<IPage> { mockPage.Object });

        var mockBrowser = new Mock<IBrowser>();
        mockBrowser.SetupGet(b => b.Contexts).Returns(new List<IBrowserContext> { mockContext.Object });

        // Act
        var result = await _tabManager.CreateNewTabAsync(mockBrowser.Object, customUrl);

        // Assert
        navigatedUrl.Should().Be(customUrl);
    }

    [Fact]
    public async Task CreateNewTabAsync_WithNoContext_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockBrowser = new Mock<IBrowser>();
        mockBrowser.SetupGet(b => b.Contexts).Returns(new List<IBrowserContext>());

        // Act & Assert
        var act = async () => await _tabManager.CreateNewTabAsync(mockBrowser.Object);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No browser contexts available*");
    }

    [Fact]
    public async Task CreateNewTabAsync_WithNullBrowser_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = async () => await _tabManager.CreateNewTabAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region CloseTabAsync Tests

    [Fact]
    public async Task CloseTabAsync_ClosesTabWhenMultipleExist()
    {
        // Arrange
        var pageToClose = CreateMockPage("https://linkedin.com/jobs", "LinkedIn Jobs");
        var otherPage = CreateMockPage("https://google.com", "Google");

        var pages = new List<IPage> { pageToClose.Object, otherPage.Object };

        var mockContext = CreateMockContext(pages);

        var mockBrowser = new Mock<IBrowser>();
        mockBrowser.SetupGet(b => b.Contexts).Returns(new List<IBrowserContext> { mockContext.Object });

        var tabInfo = new TabInfo
        {
            Page = pageToClose.Object,
            Url = "https://linkedin.com/jobs",
            Title = "LinkedIn Jobs",
            IsLinkedIn = true
        };

        // Act
        var result = await _tabManager.CloseTabAsync(mockBrowser.Object, tabInfo);

        // Assert
        result.Should().BeTrue();
        pageToClose.Verify(p => p.CloseAsync(), Times.Once);
    }

    [Fact]
    public async Task CloseTabAsync_RefusesToCloseLastTab()
    {
        // Arrange
        var singlePage = CreateMockPage("https://linkedin.com/jobs", "LinkedIn Jobs");

        var pages = new List<IPage> { singlePage.Object };

        var mockContext = CreateMockContext(pages);

        var mockBrowser = new Mock<IBrowser>();
        mockBrowser.SetupGet(b => b.Contexts).Returns(new List<IBrowserContext> { mockContext.Object });

        var tabInfo = new TabInfo
        {
            Page = singlePage.Object,
            Url = "https://linkedin.com/jobs",
            Title = "LinkedIn Jobs",
            IsLinkedIn = true
        };

        // Act
        var result = await _tabManager.CloseTabAsync(mockBrowser.Object, tabInfo);

        // Assert
        result.Should().BeFalse();
        singlePage.Verify(p => p.CloseAsync(), Times.Never);
    }

    [Fact]
    public async Task CloseTabAsync_RefusesWhenOnlyOneTabAcrossAllContexts()
    {
        // Arrange
        var singlePage = CreateMockPage("https://linkedin.com/jobs", "LinkedIn Jobs");

        var context1 = CreateMockContext(new List<IPage> { singlePage.Object });
        var context2 = CreateMockContext(new List<IPage>());

        var mockBrowser = new Mock<IBrowser>();
        mockBrowser.SetupGet(b => b.Contexts).Returns(new List<IBrowserContext> { context1.Object, context2.Object });

        var tabInfo = new TabInfo
        {
            Page = singlePage.Object,
            Url = "https://linkedin.com/jobs",
            Title = "LinkedIn Jobs",
            IsLinkedIn = true
        };

        // Act
        var result = await _tabManager.CloseTabAsync(mockBrowser.Object, tabInfo);

        // Assert
        result.Should().BeFalse();
        singlePage.Verify(p => p.CloseAsync(), Times.Never);
    }

    [Fact]
    public async Task CloseTabAsync_ClosesLastTabInContext_WhenOtherContextsHaveTabs()
    {
        // Arrange
        var pageToClose = CreateMockPage("https://linkedin.com/jobs", "LinkedIn Jobs");
        var otherPage = CreateMockPage("https://google.com", "Google");

        var context1 = CreateMockContext(new List<IPage> { pageToClose.Object });
        var context2 = CreateMockContext(new List<IPage> { otherPage.Object });

        var mockBrowser = new Mock<IBrowser>();
        mockBrowser.SetupGet(b => b.Contexts).Returns(new List<IBrowserContext> { context1.Object, context2.Object });

        var tabInfo = new TabInfo
        {
            Page = pageToClose.Object,
            Url = "https://linkedin.com/jobs",
            Title = "LinkedIn Jobs",
            IsLinkedIn = true
        };

        // Act
        var result = await _tabManager.CloseTabAsync(mockBrowser.Object, tabInfo);

        // Assert
        result.Should().BeTrue();
        pageToClose.Verify(p => p.CloseAsync(), Times.Once);
    }

    [Fact]
    public async Task CloseTabAsync_ReturnsFalse_WhenTabNotFoundInContexts()
    {
        // Arrange
        var unknownPage = CreateMockPage("https://unknown.com", "Unknown");
        var existingPage = CreateMockPage("https://linkedin.com/jobs", "LinkedIn Jobs");

        var mockContext = CreateMockContext(new List<IPage> { existingPage.Object });

        var mockBrowser = new Mock<IBrowser>();
        mockBrowser.SetupGet(b => b.Contexts).Returns(new List<IBrowserContext> { mockContext.Object });

        var tabInfo = new TabInfo
        {
            Page = unknownPage.Object,
            Url = "https://unknown.com",
            Title = "Unknown",
            IsLinkedIn = false
        };

        // Act
        var result = await _tabManager.CloseTabAsync(mockBrowser.Object, tabInfo);

        // Assert
        result.Should().BeFalse();
        unknownPage.Verify(p => p.CloseAsync(), Times.Never);
    }

    [Fact]
    public async Task CloseTabAsync_WithNullBrowser_ThrowsArgumentNullException()
    {
        // Arrange
        var tabInfo = new TabInfo
        {
            Page = new Mock<IPage>().Object,
            Url = "https://example.com",
            Title = "Example",
            IsLinkedIn = false
        };

        // Act & Assert
        var act = async () => await _tabManager.CloseTabAsync(null!, tabInfo);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CloseTabAsync_WithNullTab_ThrowsArgumentNullException()
    {
        // Arrange
        var mockBrowser = new Mock<IBrowser>();

        // Act & Assert
        var act = async () => await _tabManager.CloseTabAsync(mockBrowser.Object, null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region TabInfo Record Tests

    [Fact]
    public void TabInfo_WithLinkedInUrl_HasIsLinkedInTrue()
    {
        // Arrange & Act
        var tabInfo = new TabInfo
        {
            Page = new Mock<IPage>().Object,
            Url = "https://www.linkedin.com/jobs",
            Title = "Jobs",
            IsLinkedIn = true
        };

        // Assert
        tabInfo.IsLinkedIn.Should().BeTrue();
        tabInfo.Url.Should().Be("https://www.linkedin.com/jobs");
    }

    [Fact]
    public void TabInfo_RecordEquality_WorksCorrectly()
    {
        // Arrange
        var page = new Mock<IPage>().Object;
        
        var tab1 = new TabInfo
        {
            Page = page,
            Url = "https://linkedin.com",
            Title = "LinkedIn",
            IsLinkedIn = true
        };

        var tab2 = new TabInfo
        {
            Page = page,
            Url = "https://linkedin.com",
            Title = "LinkedIn",
            IsLinkedIn = true
        };

        // Act & Assert
        tab1.Should().Be(tab2);
    }

    #endregion

    #region Helper Methods

    private static Mock<IPage> CreateMockPage(string url, string title)
    {
        var mockPage = new Mock<IPage>();
        mockPage.SetupGet(p => p.Url).Returns(url);
        mockPage.Setup(p => p.TitleAsync()).ReturnsAsync(title);
        return mockPage;
    }

    private static Mock<IBrowserContext> CreateMockContext(List<IPage> pages)
    {
        var mockContext = new Mock<IBrowserContext>();
        mockContext.SetupGet(c => c.Pages).Returns(pages);
        
        // Set up NewPageAsync to add a new page to the list
        mockContext
            .Setup(c => c.NewPageAsync())
            .ReturnsAsync(() =>
            {
                var newPage = CreateMockPage("about:blank", "");
                pages.Add(newPage.Object);
                return newPage.Object;
            });

        return mockContext;
    }

    #endregion
}
