namespace JobCommandCenter.Harvester.Configuration;

/// <summary>
/// Configuration options for Chrome DevTools Protocol connection.
/// </summary>
public class ChromeCdpOptions
{
    /// <summary>
    /// The port number for Chrome remote debugging.
    /// </summary>
    public int Port { get; set; } = 9222;

    /// <summary>
    /// Connection timeout in seconds.
    /// </summary>
    public int ConnectionTimeoutSeconds { get; set; } = 5;
}
