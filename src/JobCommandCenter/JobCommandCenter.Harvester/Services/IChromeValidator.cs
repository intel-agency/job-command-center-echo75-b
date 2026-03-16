#nullable enable

using JobCommandCenter.Harvester.Models;

namespace JobCommandCenter.Harvester.Services;

/// <summary>
/// Validates Chrome DevTools Protocol availability before attempting connection.
/// </summary>
public interface IChromeValidator
{
    /// <summary>
    /// Checks if Chrome is running with remote debugging enabled on the configured port.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A result indicating availability status and connection details or error guidance.</returns>
    Task<ChromeValidationResult> CheckPortAvailabilityAsync(CancellationToken cancellationToken = default);
}
