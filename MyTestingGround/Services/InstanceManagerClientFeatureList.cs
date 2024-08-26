using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Channels;
using Grpc.Core;
using MyApi.Contract;
using MyTestingGround.Services;

namespace MyApi.Services;
class InstanceManagerClientFeatureList : IInstanceManagerClientFeatureList
{
    private readonly ISynchronizedFeatureListService client;
    private readonly ILogger<InstanceManagerClientFeatureList> logger;
    private CancellationTokenSource ctsInstanceManager;
    private volatile bool _disposed;
    private readonly Channel<FeatureList> locations;
    private readonly SocketConfiguration socketConfiguration;

    static CallOptions GetDefaultCallContext => new BaseMessageHeader().ToCallOptions();

    public InstanceManagerClientFeatureList(IOptions<SocketConfiguration> optionsSocketConfiguration, ISynchronizedFeatureListService client, ILogger<InstanceManagerClientFeatureList> logger)
    {
        this.client = client;
        this.logger = logger;
        socketConfiguration = optionsSocketConfiguration.Value;
        ctsInstanceManager = new();

        var options = new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.DropOldest };
        // ambiguous reference
        locations = System.Threading.Channels.Channel.CreateBounded<FeatureList>(options);
    }

    public void ReportLocation(FeatureList location)
    {
        if (socketConfiguration.IsDisabled)
            return;

        logger.LogDebug("Send location, {location}", location);
        locations.Writer.TryWrite(location);
    }

    public void Disconnect()
    {
        if (socketConfiguration.IsDisabled)
            return;

        ctsInstanceManager.Cancel();
        ctsInstanceManager = new CancellationTokenSource();
    }

    public void Connect(Action<FeatureList> locationUpdatedByServer)
    {
        if (socketConfiguration.IsDisabled)
            return;

        var ct = ctsInstanceManager.Token;
        Task.Run(async () =>
        {
            for (; ; )
            {
                try
                {
                    var options = new ConnectMessageHeaderFeatureList().ToCallOptions(ct);

                    var c = client.ConnectAsync(locations.Reader.ReadAllAsync(ct), options).WithCancellation(ct);
                    await foreach (var location in c)
                    {
                        locationUpdatedByServer(location);
                    }
                }
                catch (RpcException rpxex)
                {
                    if (rpxex.StatusCode == StatusCode.Cancelled)
                        break;
                    logger.LogWarning("Server disconnected, will try again");
                }
            }
        }, ct).ContinueWith(t =>
        {
            if (t.IsFaulted && t.Exception is { } exception)
            {
                // From: https://stackoverflow.com/a/7167719/6461844
                //Utils.Dispatch(() => throw exception);
            }
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    public async Task<IDictionary<int, AttachableProcess>> GetProcessesAsync()
    {
        var processResponse = await client.GetProcessesAsync(GetDefaultCallContext).ConfigureAwait(true);

        return processResponse;
    }

    public async Task SubscribeAsync(int processId)
    {
        if (socketConfiguration.IsDisabled)
            return;

        var subscriptionRequest = new SubscriptionRequest() { SubscribeToProcessId = processId };
        await client.SubscribeAsync(subscriptionRequest, GetDefaultCallContext).ConfigureAwait(false);
    }
}
