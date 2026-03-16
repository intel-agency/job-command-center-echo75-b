namespace JobCommandCenter.Harvester.Services;

/// <summary>
/// Validates Chrome remote debugging port availability.
/// </summary>
public interface IChromeValidator
{
    /// <summary>
    /// Checks if Chrome is running with remote debugging enabled on the configured port.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating availability status and any error guidance.</returns>
    Task<ChromeValidationResult> CheckPortAvailabilityAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of Chrome validation check.
/// </summary>
public record ChromeValidationResult
{
    /// <summary>
    /// Whether Chrome CDP is available.
    /// </summary>
    public required bool IsAvailable { get; init; }

    /// <summary>
    /// Chrome version if available.
    /// </summary>
    public string? ChromeVersion { get; init; }

    /// <summary>
    /// User data directory if available.
    /// </summary>
    public string? UserDataDirectory { get; init; }

    /// <summary>
    /// Error message if not available.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Guidance for resolving the issue.
    /// </summary>
    public string? ResolutionGuidance { get; init; }
}
