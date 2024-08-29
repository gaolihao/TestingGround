using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Channels;
using Grpc.Core;
using MyApi.Contract;
using MyTestingGround.Services;

namespace MyApi.Services
{
    public class InstanceManagerClient : IInstanceManagerClient, IDisposable
    {
        private readonly ISynchronizedScrollingService client;
        private readonly ILogger<InstanceManagerClient> logger;
        private CancellationTokenSource ctsInstanceManager;
        private volatile bool _disposed;
        private readonly Channel<Location> locations;
        private readonly SocketConfiguration socketConfiguration;

        static CallOptions GetDefaultCallContext => new BaseMessageHeader().ToCallOptions();

        public InstanceManagerClient(IOptions<SocketConfiguration> optionsSocketConfiguration, ISynchronizedScrollingService client, ILogger<InstanceManagerClient> logger)
        {
            this.client = client;
            this.logger = logger;
            socketConfiguration = optionsSocketConfiguration.Value;
            ctsInstanceManager = new();

            var options = new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.DropOldest };
            locations = System.Threading.Channels.Channel.CreateBounded<Location>(options);
        }

        public void ReportLocation(Location location)
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

        public void Connect(string filename, Action<Location> locationUpdatedByServer)
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
                        var options = new ConnectMessageHeader(filename).ToCallOptions(ct);

                        var c = client.ConnectAsync(locations.Reader.ReadAllAsync(ct), options).WithCancellation(ct);
                        await foreach (var location in c)
                        {
                            //Utils.Dispatch(() => locationUpdatedByServer(location));
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
                    // Utils.Dispatch(() => throw exception);
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

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _disposed = true;
                ctsInstanceManager.Cancel();
                ctsInstanceManager.Dispose();
            }
        }
        #endregion
    }
}
