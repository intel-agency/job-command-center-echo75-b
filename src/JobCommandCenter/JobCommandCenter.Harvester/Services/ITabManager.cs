#nullable enable

using Microsoft.Playwright;

namespace JobCommandCenter.Harvester.Services;

/// <summary>
/// Manages browser tabs for LinkedIn navigation.
/// </summary>
public interface ITabManager
{
    /// <summary>
    /// Gets all open browser tabs/pages.
    /// </summary>
    /// <param name="browser">The connected browser instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of tab information.</returns>
    Task<IReadOnlyList<TabInfo>> GetTabsAsync(IBrowser browser, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds an existing LinkedIn job page tab.
    /// </summary>
    /// <param name="browser">The connected browser instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The LinkedIn tab if found, otherwise null.</returns>
    Task<TabInfo?> FindLinkedInTabAsync(IBrowser browser, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new tab and navigates to LinkedIn jobs.
    /// </summary>
    /// <param name="browser">The connected browser instance.</param>
    /// <param name="url">Optional specific URL to navigate to.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created tab.</returns>
    Task<TabInfo> CreateNewTabAsync(IBrowser browser, string? url = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Closes a tab with safety checks (won't close last tab).
    /// </summary>
    /// <param name="browser">The connected browser instance.</param>
    /// <param name="tab">The tab to close.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if closed, false if refused (e.g., last tab).</returns>
    Task<bool> CloseTabAsync(IBrowser browser, TabInfo tab, CancellationToken cancellationToken = default);
}

/// <summary>
/// Information about a browser tab.
/// </summary>
public record TabInfo
{
    /// <summary>
    /// The page instance.
    /// </summary>
    public required IPage Page { get; init; }
    
    /// <summary>
    /// The tab URL.
    /// </summary>
    public string Url { get; init; } = string.Empty;
    
    /// <summary>
    /// The page title.
    /// </summary>
    public string? Title { get; init; }
    
    /// <summary>
    /// Whether this is a LinkedIn page.
    /// </summary>
    public bool IsLinkedIn { get; init; }
}
