using Grpc.Core;
using ProtoBuf.Grpc;
using System.Runtime.CompilerServices;
using MyApi.Contract;

namespace MyApi.Services;

/// <inheritdoc/>
public class SynchronizedFeatureListService
{
    private readonly SynchronizedFeatureListRepository synchronizedFeatureListRepository;
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SynchronizedFeatureListService"/> class.
    /// </summary>
    /// <param name="synchronizedScrollingRepository">Repository.</param>
    /// <param name="loggerFactory">Logger Factory.</param>
    public SynchronizedFeatureListService(SynchronizedFeatureListRepository synchronizedScrollingRepository, ILoggerFactory loggerFactory)
    {
        logger = loggerFactory.CreateLogger<SynchronizedFeatureListService>();
        this.synchronizedFeatureListRepository = synchronizedScrollingRepository;
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<FeatureList> ConnectAsync(IAsyncEnumerable<FeatureList> featureLists, CallContext context = default)
    {
        return ConnectAsync(new ConnectMessageHeaderFeatureList(context), featureLists, context.CancellationToken);
    }

    /// <inheritdoc/>
    public Task<IDictionary<int, AttachableProcess>> GetProcessesAsync(CallContext context)
    {
        var processId = GetCallingProcess(context);

        // Check if the process has any subscribers
        var subscribers = synchronizedFeatureListRepository.GetPathAndQueue(processId).Subscribers;
        if (subscribers.Count > 0)
        {
            var message = $"Please detach process(es) {string.Join(", ", subscribers)} first";
            throw new RpcException(new Status(StatusCode.AlreadyExists, message));
        }

        IDictionary<int, AttachableProcess> dict = synchronizedFeatureListRepository.All
            .Where(z => z.Key != processId)
            .ToDictionary(
            kv => kv.Key,
            kv => new AttachableProcess("", !synchronizedFeatureListRepository.IsSubscriber(kv.Key)));
        return Task.FromResult(dict);
    }

    /// <inheritdoc/>
    public Task SubscribeAsync(SubscriptionRequest subscriptionRequest, CallContext context = default)
    {
        var processId = GetCallingProcess(context);

        logger.LogInformation("Subscribe (pid: {processId}) to {subscribeToProcessId}", processId, subscriptionRequest.SubscribeToProcessId);

        // Check if we subscribe to subscriber
        if (synchronizedFeatureListRepository.IsSubscriber(subscriptionRequest.SubscribeToProcessId))
        {
            throw new RpcException(new Status(StatusCode.AlreadyExists, $"Process {subscriptionRequest.SubscribeToProcessId} already subscribed to another process."));
        }

        // Add subscriber to parent
        var pathAndQueue = synchronizedFeatureListRepository.GetPathAndQueue(subscriptionRequest.SubscribeToProcessId);
        pathAndQueue.Subscribers.Add(processId);

        // Inform subscriber of parent's last location
        //var subscriberLocationSource = synchronizedFeatureListRepository.GetPathAndQueue(processId);
        //subscriberLocationSource.Publish(pathAndQueue.LastLocation);

        return Task.CompletedTask;
    }

    private static int GetCallingProcess(CallContext context)
    {
        return new BaseMessageHeader(context).ProcessId;
    }

    private async Task ReceiveAsync(int processId, FeatureSource pq, IAsyncEnumerable<FeatureList> locations, CancellationToken ct = default)
    {
        // Should not be catching IOException https://github.com/grpc/grpc-dotnet/issues/1452
        try
        {
            await foreach (var instanceLocation in locations.WithCancellation(ct))
            {
                ProcessNewLocation(processId, pq, instanceLocation);
            }
        }
        catch (IOException ex)
        {
            // https://github.com/grpc/grpc-dotnet/issues/1452
            if (!ex.Message.Contains("aborted", StringComparison.InvariantCultureIgnoreCase))
            {
                throw;
            }
        }
    }

    private void ProcessNewLocation(int processId, FeatureSource pq, FeatureList instanceLocation)
    {
        if (instanceLocation.MessageType == MessageType.Unsubscribe)
        {
            synchronizedFeatureListRepository.Unsubscribe(processId);
        }
        else
        {
            // pq.LastLocation = instanceLocation;
            synchronizedFeatureListRepository.InformSubscribers(processId, instanceLocation, pq);
            synchronizedFeatureListRepository.InformParent(processId, instanceLocation);
        }
    }

    private async IAsyncEnumerable<FeatureList> ConnectAsync(ConnectMessageHeaderFeatureList header, IAsyncEnumerable<FeatureList> featureLists, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var processId = header.ProcessId;

        // logger.LogInformation("Client connected {filePath} (pid: {processId}) with initial location {loc}", filePath, processId, initialLocation);

        var featureListSource = synchronizedFeatureListRepository.GetPathAndQueue(processId);
        // ProcessNewLocation(processId, featureListSource);
        var receiveTask = Task.Run(async () => await ReceiveAsync(processId, featureListSource, featureLists), ct);

        ////return pq.LocationQueue.ReadAllAsync(ct);

        while (!ct.IsCancellationRequested)
        {
            FeatureList featureList;
            try
            {
                featureList = await featureListSource.ReadAsync(ct);
            }
            catch (OperationCanceledException)
            {
                RemoveProcess(processId, featureListSource);
                yield break;
            }

            logger.LogInformation("Process (pid: {processId}) gets location: {location}", processId, featureList);
            yield return featureList;
        }

        RemoveProcess(processId, featureListSource);

        logger.LogInformation("Server: done!");
    }

    private void RemoveProcess(int processId, FeatureSource featureSource)
    {
        logger.LogInformation("Client disconnected (pid: {processId})", processId);
        synchronizedFeatureListRepository.InformSubscribers(processId, new FeatureList { MessageType = MessageType.Unsubscribe }, featureSource);
        synchronizedFeatureListRepository.Delete(processId);
    }
}
