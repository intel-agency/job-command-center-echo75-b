#nullable enable

namespace JobCommandCenter.Harvester.Models;

/// <summary>
/// Represents information about a browser tab.
/// </summary>
public sealed record TabInfo
{
    /// <summary>
    /// Gets the unique identifier for the tab (Playwright page GUID).
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the URL of the tab, if available.
    /// </summary>
    public string? Url { get; init; }

    /// <summary>
    /// Gets the title of the tab, if available.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Gets a value indicating whether this tab is a LinkedIn page.
    /// </summary>
    public bool IsLinkedIn => Url?.Contains("linkedin.com", StringComparison.OrdinalIgnoreCase) == true;

    /// <summary>
    /// Gets a value indicating whether this tab is a LinkedIn jobs page.
    /// </summary>
    public bool IsJobPage => Url?.Contains("linkedin.com/jobs", StringComparison.OrdinalIgnoreCase) == true;

    /// <summary>
    /// Creates a TabInfo from Playwright page information.
    /// </summary>
    /// <param name="pageId">The page identifier.</param>
    /// <param name="url">The page URL.</param>
    /// <param name="title">The page title.</param>
    /// <returns>A new TabInfo instance.</returns>
    public static TabInfo FromPage(string pageId, string? url, string? title) => new()
    {
        Id = pageId,
        Url = url,
        Title = title
    };
}
