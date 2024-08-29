using System.Threading.Channels;
using MyApi.Contract;

namespace MyApi;

/// <summary>
/// Holds file path information as well as the location queue.
/// </summary>
public class LocationSource
{
    private readonly Channel<Location> incomingLocation;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocationSource"/> class.
    /// </summary>
    /// <param name="filepath">The file path.</param>
    public LocationSource(string filepath)
    {
        FilePath = filepath;
        var options = new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.DropOldest };
        incomingLocation = Channel.CreateBounded<Location>(options);
    }

    /// <summary>
    /// Gets the file path.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// Gets or sets the last location.
    /// </summary>
    public Location LastLocation { get; set; } = new Location();

    /// <summary>
    /// Gets the subscribers.
    /// </summary>
    public HashSet<int> Subscribers { get; } = [];

    /// <summary>
    /// Publishes a location.
    /// </summary>
    /// <param name="location">The location.</param>
    public void Publish(Location location)
        => _ = incomingLocation.Writer.TryWrite(location);

    /// <summary>
    /// Receive <see cref="Location"/> data.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A ValudTask.</returns>
    public ValueTask<Location> ReadAsync(CancellationToken ct = default)
        => incomingLocation.Reader.ReadAsync(ct);
}
