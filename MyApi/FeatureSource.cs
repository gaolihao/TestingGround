using System.Threading.Channels;
using MyApi.Contract;

namespace MyApi;

public class FeatureSource
{
    private readonly Channel<FeatureList> incomingLocation;

    public FeatureSource() 
    {
        var options = new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.DropOldest };
        incomingLocation = Channel.CreateBounded<FeatureList>(options);
    }

    public HashSet<int> Subscribers { get; } = [];

    public void Publish(FeatureList feature) 
    {
        incomingLocation.Writer.TryWrite(feature);
    }

    public ValueTask<FeatureList> ReadAsync(CancellationToken ct = default)
    {
        return incomingLocation.Reader.ReadAsync(ct);
    }
}
