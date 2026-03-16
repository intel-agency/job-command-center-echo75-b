#nullable enable

using JobCommandCenter.Harvester.Models;

namespace JobCommandCenter.Harvester.Services;

/// <summary>
/// Factory interface for establishing Chrome DevTools Protocol connections.
/// </summary>
public interface ICdpConnectionFactory
{
    /// <summary>
    /// Establishes a CDP connection to Chrome and validates the session.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A result containing the connected browser or error information.</returns>
    Task<CdpConnectionResult> ConnectAsync(CancellationToken cancellationToken = default);
}
