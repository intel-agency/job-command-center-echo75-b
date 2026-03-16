#nullable enable

using Microsoft.Playwright;

namespace JobCommandCenter.Harvester.Services;

/// <summary>
/// Factory for creating Chrome DevTools Protocol browser connections.
/// </summary>
public interface ICdpConnectionFactory
{
    /// <summary>
    /// Establishes a CDP connection to Chrome.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A connected browser instance.</returns>
    Task<IBrowser> ConnectAsync(CancellationToken cancellationToken = default);
}
