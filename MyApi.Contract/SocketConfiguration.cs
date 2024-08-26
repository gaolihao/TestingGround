namespace MyApi.Contract;

/// <summary>
/// Represents socket configuration.
/// </summary>
public class SocketConfiguration
{
    /// <summary>
    /// Gets or sets Unix socket filename.
    /// </summary>
    public string Filename { get; set; } = default!;

    /// <summary>
    /// Gets or sets the port number when unix sockets are not supported.
    /// </summary>
    public int HttpPort { get; set; } = 5232;

    /// <summary>
    /// Gets or sets a value indicating whether data should be reported to the instance manager.
    /// </summary>
    public bool IsDisabled { get; set; }
}
