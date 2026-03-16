#nullable enable

using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace JobCommandCenter.Harvester.Services;

/// <summary>
/// Default URL for LinkedIn jobs page.
/// </summary>
public static class LinkedInUrls
{
    /// <summary>
    /// The default LinkedIn jobs URL.
    /// </summary>
    public const string JobsPage = "https://www.linkedin.com/jobs/";
}

/// <summary>
/// Manages browser tabs for LinkedIn navigation using Playwright.
/// </summary>
public sealed class TabManager : ITabManager
{
    private readonly ILogger<TabManager> _logger;

    /// <summary>
    /// Creates a new instance of TabManager.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public TabManager(ILogger<TabManager> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TabInfo>> GetTabsAsync(IBrowser browser, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(browser);

        _logger.LogDebug("Getting all tabs from browser");

        var tabs = new List<TabInfo>();
        var contexts = browser.Contexts;

        _logger.LogDebug("Found {ContextCount} browser contexts", contexts.Count);

        foreach (var context in contexts)
        {
            var pages = context.Pages;
            _logger.LogDebug("Context has {PageCount} pages", pages.Count);

            foreach (var page in pages)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var tabInfo = await CreateTabInfoAsync(page, cancellationToken).ConfigureAwait(false);
                tabs.Add(tabInfo);
            }
        }

        _logger.LogInformation("Retrieved {TabCount} total tabs", tabs.Count);
        return tabs.AsReadOnly();
    }

    /// <inheritdoc />
    public async Task<TabInfo?> FindLinkedInTabAsync(IBrowser browser, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(browser);

        _logger.LogDebug("Searching for LinkedIn tab");

        var tabs = await GetTabsAsync(browser, cancellationToken).ConfigureAwait(false);
        var linkedInTab = tabs.FirstOrDefault(t => t.IsLinkedIn);

        if (linkedInTab is not null)
        {
            _logger.LogInformation(
                "Found LinkedIn tab: {Url} - {Title}",
                linkedInTab.Url,
                linkedInTab.Title ?? "(no title)");
        }
        else
        {
            _logger.LogDebug("No LinkedIn tab found among {TabCount} tabs", tabs.Count);
        }

        return linkedInTab;
    }

    /// <inheritdoc />
    public async Task<TabInfo> CreateNewTabAsync(IBrowser browser, string? url = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(browser);

        var targetUrl = url ?? LinkedInUrls.JobsPage;
        _logger.LogInformation("Creating new tab with URL: {Url}", targetUrl);

        // Get the first available context or throw if none exists
        var context = browser.Contexts.FirstOrDefault();
        if (context is null)
        {
            _logger.LogWarning("No browser contexts available to create new tab");
            throw new InvalidOperationException("No browser contexts available. The browser connection may be invalid.");
        }

        var page = await context.NewPageAsync().ConfigureAwait(false);
        _logger.LogDebug("Created new page in browser context");

        try
        {
            await page.GotoAsync(targetUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
                Timeout = 30000
            }).ConfigureAwait(false);

            _logger.LogInformation("Navigated to {Url}", targetUrl);
        }
        catch (PlaywrightException ex)
        {
            _logger.LogError(ex, "Failed to navigate to {Url}", targetUrl);
            // Still return the tab even if navigation failed - the caller can handle the error
        }

        var tabInfo = await CreateTabInfoAsync(page, cancellationToken).ConfigureAwait(false);
        return tabInfo;
    }

    /// <inheritdoc />
    public async Task<bool> CloseTabAsync(IBrowser browser, TabInfo tab, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(browser);
        ArgumentNullException.ThrowIfNull(tab);

        // Count total pages across all contexts
        var totalPages = browser.Contexts.Sum(c => c.Pages.Count);
        
        if (totalPages <= 1)
        {
            _logger.LogWarning(
                "Refusing to close tab - only {PageCount} page(s) open across all contexts",
                totalPages);
            return false;
        }

        // Find which context contains this page
        IBrowserContext? targetContext = null;
        foreach (var context in browser.Contexts)
        {
            if (context.Pages.Contains(tab.Page))
            {
                targetContext = context;
                break;
            }
        }

        if (targetContext is null)
        {
            _logger.LogWarning("Tab not found in any browser context");
            return false;
        }

        // Check if this is the last page in its context
        if (targetContext.Pages.Count <= 1)
        {
            // Only refuse if it's the only page AND it's the only context
            if (browser.Contexts.Count <= 1)
            {
                _logger.LogWarning(
                    "Refusing to close tab - it is the last tab in the only browser context");
                return false;
            }
        }

        _logger.LogInformation(
            "Closing tab: {Url} - {Title}",
            tab.Url,
            tab.Title ?? "(no title)");

        try
        {
            await tab.Page.CloseAsync().ConfigureAwait(false);
            _logger.LogDebug("Tab closed successfully");
            return true;
        }
        catch (PlaywrightException ex)
        {
            _logger.LogError(ex, "Failed to close tab: {Url}", tab.Url);
            return false;
        }
    }

    /// <summary>
    /// Creates a TabInfo record from a page instance.
    /// </summary>
    /// <param name="page">The page to create tab info for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created TabInfo.</returns>
    private static async Task<TabInfo> CreateTabInfoAsync(IPage page, CancellationToken cancellationToken)
    {
        string url = page.Url;
        string? title = null;

        try
        {
            // Try to get the title, but don't fail if it's not available
            title = await page.TitleAsync().ConfigureAwait(false);
        }
        catch (PlaywrightException)
        {
            // Title not available, leave as null
        }

        var isLinkedIn = IsLinkedInUrl(url);

        return new TabInfo
        {
            Page = page,
            Url = url,
            Title = title,
            IsLinkedIn = isLinkedIn
        };
    }

    /// <summary>
    /// Determines if a URL is a LinkedIn page.
    /// </summary>
    /// <param name="url">The URL to check.</param>
    /// <returns>True if the URL is a LinkedIn page.</returns>
    private static bool IsLinkedInUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        try
        {
            var uri = new Uri(url);
            return uri.Host.EndsWith("linkedin.com", StringComparison.OrdinalIgnoreCase);
        }
        catch (UriFormatException)
        {
            // Invalid URL, not a LinkedIn page
            return false;
        }
    }
}
