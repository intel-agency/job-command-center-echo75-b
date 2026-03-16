#nullable enable

using JobCommandCenter.Harvester.Models;

namespace JobCommandCenter.Harvester.Services;

/// <summary>
/// Interface for managing browser tabs via Chrome DevTools Protocol.
/// </summary>
public interface ITabManager
{
    /// <summary>
    /// Gets all open tabs across all browser contexts.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A read-only list of tab information.</returns>
    Task<IReadOnlyList<TabInfo>> GetTabsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds an existing LinkedIn job page tab.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The LinkedIn tab if found, otherwise null.</returns>
    Task<TabInfo?> FindLinkedInTabAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new tab and navigates to the specified URL.
    /// </summary>
    /// <param name="url">The URL to navigate to.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The newly created tab information.</returns>
    Task<TabInfo> CreateNewTabAsync(string url, CancellationToken cancellationToken = default);

    /// <summary>
    /// Closes a tab by its identifier.
    /// </summary>
    /// <param name="tabId">The tab identifier to close.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>True if the tab was closed, false if it couldn't be closed (e.g., last tab).</returns>
    Task<bool> CloseTabAsync(string tabId, CancellationToken cancellationToken = default);
}
