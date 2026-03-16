#nullable enable

using System.Diagnostics;
using JobCommandCenter.Harvester.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace JobCommandCenter.Harvester.Services;

/// <summary>
/// Manages browser tabs via Chrome DevTools Protocol using Playwright.
/// </summary>
public sealed class TabManager : ITabManager
{
    private readonly IBrowser _browser;
    private readonly ILogger<TabManager> _logger;

    /// <summary>
    /// Creates a new instance of TabManager.
    /// </summary>
    /// <param name="browser">The connected Playwright browser instance.</param>
    /// <param name="logger">The logger instance.</param>
    public TabManager(IBrowser browser, ILogger<TabManager> logger)
    {
        _browser = browser ?? throw new ArgumentNullException(nameof(browser));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TabInfo>> GetTabsAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogDebug("Getting all open tabs");

        // Check for cancellation before starting work
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var tabs = new List<TabInfo>();
            var contexts = _browser.Contexts;

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

            stopwatch.Stop();
            _logger.LogInformation(
                "Retrieved {TabCount} tabs from {ContextCount} contexts in {Duration}ms",
                tabs.Count,
                contexts.Count,
                stopwatch.ElapsedMilliseconds);

            return tabs.AsReadOnly();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("GetTabsAsync was cancelled");
            throw;
        }
        catch (PlaywrightException ex)
        {
            _logger.LogError(ex, "Playwright error while getting tabs");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<TabInfo?> FindLinkedInTabAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogDebug("Searching for LinkedIn job tab");

        try
        {
            var tabs = await GetTabsAsync(cancellationToken).ConfigureAwait(false);
            var linkedInTab = tabs.FirstOrDefault(t => t.IsJobPage);

            stopwatch.Stop();

            if (linkedInTab is not null)
            {
                _logger.LogInformation(
                    "Found LinkedIn job tab {TabId} with URL {Url} in {Duration}ms",
                    linkedInTab.Id,
                    linkedInTab.Url,
                    stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogInformation(
                    "No LinkedIn job tab found among {TabCount} tabs in {Duration}ms",
                    tabs.Count,
                    stopwatch.ElapsedMilliseconds);
            }

            return linkedInTab;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("FindLinkedInTabAsync was cancelled");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<TabInfo> CreateNewTabAsync(string url, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Creating new tab with URL {Url}", url);

        try
        {
            // Get the default context (first context when connecting via CDP)
            var context = _browser.Contexts.FirstOrDefault();
            if (context is null)
            {
                _logger.LogWarning("No browser context available, cannot create tab");
                throw new InvalidOperationException("No browser context available. Ensure Chrome is connected via CDP.");
            }

            var page = await context.NewPageAsync().ConfigureAwait(false);
            _logger.LogDebug("Created new page {PageId}", page.GetHashCode());

            // Navigate to the URL
            var response = await page.GotoAsync(url, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
                Timeout = 30000 // 30 second timeout
            }).ConfigureAwait(false);

            if (response is null)
            {
                _logger.LogWarning("Navigation to {Url} returned null response", url);
            }
            else if (!response.Ok)
            {
                _logger.LogWarning(
                    "Navigation to {Url} returned status {Status}",
                    url,
                    response.Status);
            }

            var tabInfo = await CreateTabInfoAsync(page, cancellationToken).ConfigureAwait(false);

            stopwatch.Stop();
            _logger.LogInformation(
                "Created new tab {TabId} with URL {Url} in {Duration}ms",
                tabInfo.Id,
                tabInfo.Url,
                stopwatch.ElapsedMilliseconds);

            return tabInfo;
        }
        catch (PlaywrightException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Playwright error while creating tab for URL {Url}", url);
            throw;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("CreateNewTabAsync was cancelled");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> CloseTabAsync(string tabId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tabId);

        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Attempting to close tab {TabId}", tabId);

        try
        {
            // Find the page by ID
            IPage? targetPage = null;
            IBrowserContext? targetContext = null;

            foreach (var context in _browser.Contexts)
            {
                foreach (var page in context.Pages)
                {
                    var pageId = GetPageIdentifier(page);
                    if (pageId == tabId)
                    {
                        targetPage = page;
                        targetContext = context;
                        break;
                    }
                }

                if (targetPage is not null)
                    break;
            }

            if (targetPage is null)
            {
                _logger.LogWarning("Tab {TabId} not found", tabId);
                return false;
            }

            // Safety check: don't close the last tab in a context
            if (targetContext is not null && targetContext.Pages.Count <= 1)
            {
                _logger.LogWarning(
                    "Refusing to close tab {TabId} - it is the last tab in the context",
                    tabId);
                return false;
            }

            await targetPage.CloseAsync(new PageCloseOptions
            {
                RunBeforeUnload = true
            }).ConfigureAwait(false);

            stopwatch.Stop();
            _logger.LogInformation(
                "Closed tab {TabId} in {Duration}ms",
                tabId,
                stopwatch.ElapsedMilliseconds);

            return true;
        }
        catch (PlaywrightException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Playwright error while closing tab {TabId}", tabId);
            return false;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("CloseTabAsync was cancelled");
            throw;
        }
    }

    /// <summary>
    /// Creates a TabInfo from a Playwright page.
    /// </summary>
    private static async Task<TabInfo> CreateTabInfoAsync(IPage page, CancellationToken cancellationToken)
    {
        var id = GetPageIdentifier(page);
        string? url = null;
        string? title = null;

        try
        {
            url = page.Url;
        }
        catch (PlaywrightException)
        {
            // Page might be closed or inaccessible
        }

        try
        {
            title = await page.TitleAsync().ConfigureAwait(false);
        }
        catch (PlaywrightException)
        {
            // Page might be closed or inaccessible
        }

        return TabInfo.FromPage(id, url, title);
    }

    /// <summary>
    /// Gets a unique identifier for a page.
    /// Uses the page's hash code as a string identifier.
    /// </summary>
    private static string GetPageIdentifier(IPage page)
    {
        // Playwright pages don't have a built-in GUID, so we use the object's hash code
        // In a real implementation, you might track pages in a dictionary with generated GUIDs
        return page.GetHashCode().ToString();
    }
}
