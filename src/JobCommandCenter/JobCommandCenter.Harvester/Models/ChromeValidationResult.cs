#nullable enable

namespace JobCommandCenter.Harvester.Models;

/// <summary>
/// Represents the result of Chrome DevTools Protocol port availability validation.
/// </summary>
public sealed record ChromeValidationResult
{
    /// <summary>
    /// Gets a value indicating whether Chrome is available and ready for CDP connection.
    /// </summary>
    public required bool IsAvailable { get; init; }

    /// <summary>
    /// Gets the Chrome browser version if available.
    /// </summary>
    public string? ChromeVersion { get; init; }

    /// <summary>
    /// Gets the Chrome user data directory path if available.
    /// </summary>
    public string? UserDataDir { get; init; }

    /// <summary>
    /// Gets the error message if Chrome is not available.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Gets user-friendly guidance for resolving connection issues.
    /// </summary>
    public string? GuidanceMessage { get; init; }

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    public static ChromeValidationResult Success(string chromeVersion, string? userDataDir = null) => new()
    {
        IsAvailable = true,
        ChromeVersion = chromeVersion,
        UserDataDir = userDataDir
    };

    /// <summary>
    /// Creates a failed validation result.
    /// </summary>
    public static ChromeValidationResult Failure(string errorMessage, string guidanceMessage) => new()
    {
        IsAvailable = false,
        ErrorMessage = errorMessage,
        GuidanceMessage = guidanceMessage
    };
}
