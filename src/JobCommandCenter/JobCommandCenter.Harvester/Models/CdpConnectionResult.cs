#nullable enable

using Microsoft.Playwright;

namespace JobCommandCenter.Harvester.Models;

/// <summary>
/// Represents the result of a Chrome DevTools Protocol connection attempt.
/// </summary>
public sealed record CdpConnectionResult
{
    /// <summary>
    /// Gets a value indicating whether the connection was successful.
    /// </summary>
    public required bool IsConnected { get; init; }

    /// <summary>
    /// Gets the connected Playwright browser instance if successful.
    /// </summary>
    public IBrowser? Browser { get; init; }

    /// <summary>
    /// Gets the Chrome browser version string if connected.
    /// </summary>
    public string? BrowserVersion { get; init; }

    /// <summary>
    /// Gets the Chrome user data directory path if available.
    /// </summary>
    public string? UserDataDir { get; init; }

    /// <summary>
    /// Gets the error message if the connection failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Gets user-friendly guidance for resolving connection issues.
    /// </summary>
    public string? GuidanceMessage { get; init; }

    /// <summary>
    /// Creates a successful connection result.
    /// </summary>
    /// <param name="browser">The connected Playwright browser instance.</param>
    /// <param name="browserVersion">The browser version string.</param>
    /// <param name="userDataDir">The user data directory path (optional).</param>
    /// <returns>A successful CdpConnectionResult.</returns>
    public static CdpConnectionResult Success(IBrowser browser, string browserVersion, string? userDataDir = null) => new()
    {
        IsConnected = true,
        Browser = browser,
        BrowserVersion = browserVersion,
        UserDataDir = userDataDir
    };

    /// <summary>
    /// Creates a failed connection result.
    /// </summary>
    /// <param name="errorMessage">The error message describing the failure.</param>
    /// <param name="guidanceMessage">User-friendly guidance for resolving the issue.</param>
    /// <returns>A failed CdpConnectionResult.</returns>
    public static CdpConnectionResult Failure(string errorMessage, string guidanceMessage) => new()
    {
        IsConnected = false,
        ErrorMessage = errorMessage,
        GuidanceMessage = guidanceMessage
    };
}
