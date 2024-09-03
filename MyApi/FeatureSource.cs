using System.Threading.Channels;
using MyApi.Contract;

namespace MyApi;

/// <summary>
/// Holds file path information as well as the location queue.
/// </summary>
public class FeatureSource
{
    private readonly Channel<FeatureList> incomingLocation;

    /// <summary>
    /// Initializes a new instance of the <see cref="FeatureSource"/> class.
    /// </summary>
    /// <param name="filepath">The file path.</param>
    public FeatureSource(string filepath)
    {
        FilePath = filepath;
        var options = new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.DropOldest };
        incomingLocation = Channel.CreateBounded<FeatureList>(options);
    }

    /// <summary>
    /// Gets the file path.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// Gets or sets the last location.
    /// </summary>
    public FeatureList LastLocation { get; set; } = new FeatureList();

    /// <summary>
    /// Gets the subscribers.
    /// </summary>
    public HashSet<int> Subscribers { get; } = [];

    /// <summary>
    /// Publishes a location.
    /// </summary>
    /// <param name="location">The location.</param>
    public void Publish(FeatureList location)
        => _ = incomingLocation.Writer.TryWrite(location);

    /// <summary>
    /// Receive <see cref="FeatureList"/> data.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A ValudTask.</returns>
    public ValueTask<FeatureList> ReadAsync(CancellationToken ct = default)
        => incomingLocation.Reader.ReadAsync(ct);
}
