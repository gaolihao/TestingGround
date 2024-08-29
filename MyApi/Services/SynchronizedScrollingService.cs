using Grpc.Core;
using MyApi.Contract;
using ProtoBuf.Grpc;
using System.Runtime.CompilerServices;

namespace MyApi.Services;

/// <inheritdoc/>
public class SynchronizedScrollingService : ISynchronizedScrollingService
{
    private readonly SynchronizedScrollingRepository synchronizedScrollingRepository;
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SynchronizedScrollingService"/> class.
    /// </summary>
    /// <param name="synchronizedScrollingRepository">Repository.</param>
    /// <param name="loggerFactory">Logger Factory.</param>
    public SynchronizedScrollingService(SynchronizedScrollingRepository synchronizedScrollingRepository, ILoggerFactory loggerFactory)
    {
        logger = loggerFactory.CreateLogger<SynchronizedScrollingService>();
        this.synchronizedScrollingRepository = synchronizedScrollingRepository;
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<Location> ConnectAsync(IAsyncEnumerable<Location> locations, CallContext context = default)
        => ConnectAsync(new ConnectMessageHeader(context), locations, context.CancellationToken);

    /// <inheritdoc/>
    public Task<IDictionary<int, AttachableProcess>> GetProcessesAsync(CallContext context)
    {
        var processId = GetCallingProcess(context);

        // Check if the process has any subscribers
        var subscribers = synchronizedScrollingRepository.GetPathAndQueue(processId).Subscribers;
        if (subscribers.Count > 0)
        {
            var message = $"Please detach process(es) {string.Join(", ", subscribers)} first";
            throw new RpcException(new Status(StatusCode.AlreadyExists, message));
        }

        IDictionary<int, AttachableProcess> dict = synchronizedScrollingRepository.All
            .Where(z => z.Key != processId)
            .ToDictionary(
            kv => kv.Key,
            kv => new AttachableProcess(kv.Value.FilePath, !synchronizedScrollingRepository.IsSubscriber(kv.Key)));
        return Task.FromResult(dict);
    }

    /// <inheritdoc/>
    public Task SubscribeAsync(SubscriptionRequest subscriptionRequest, CallContext context = default)
    {
        var processId = GetCallingProcess(context);

        logger.LogInformation("Subscribe (pid: {processId}) to {subscribeToProcessId}", processId, subscriptionRequest.SubscribeToProcessId);

        // Check if we subscribe to subscriber
        if (synchronizedScrollingRepository.IsSubscriber(subscriptionRequest.SubscribeToProcessId))
        {
            throw new RpcException(new Status(StatusCode.AlreadyExists, $"Process {subscriptionRequest.SubscribeToProcessId} already subscribed to another process."));
        }

        // Add subscriber to parent
        var pathAndQueue = synchronizedScrollingRepository.GetPathAndQueue(subscriptionRequest.SubscribeToProcessId);
        pathAndQueue.Subscribers.Add(processId);

        // Inform subscriber of parent's last location
        var subscriberLocationSource = synchronizedScrollingRepository.GetPathAndQueue(processId);
        subscriberLocationSource.Publish(pathAndQueue.LastLocation);

        return Task.CompletedTask;
    }

    private static int GetCallingProcess(CallContext context)
        => new BaseMessageHeader(context).ProcessId;

    private async Task ReceiveAsync(int processId, LocationSource pq, IAsyncEnumerable<Location> locations, CancellationToken ct = default)
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

    private void ProcessNewLocation(int processId, LocationSource pq, Location instanceLocation)
    {
        if (instanceLocation.MessageType == MessageType.Unsubscribe)
        {
            synchronizedScrollingRepository.Unsubscribe(processId);
        }
        else
        {
            pq.LastLocation = instanceLocation;
            synchronizedScrollingRepository.InformSubscribers(processId, instanceLocation, pq);
            synchronizedScrollingRepository.InformParent(processId, instanceLocation);
        }
    }

    private async IAsyncEnumerable<Location> ConnectAsync(ConnectMessageHeader header, IAsyncEnumerable<Location> locations, [EnumeratorCancellation] CancellationToken ct = default)
    {
        (var processId, var filePath, var initialLocation) = header;

        logger.LogInformation("Client connected {filePath} (pid: {processId}) with initial location {loc}", filePath, processId, initialLocation);

        var locationSource = synchronizedScrollingRepository.GetPathAndQueue(processId, filePath);
        ProcessNewLocation(processId, locationSource, initialLocation);
        var receiveTask = Task.Run(async () => await ReceiveAsync(processId, locationSource, locations), ct);

        ////return pq.LocationQueue.ReadAllAsync(ct);

        while (!ct.IsCancellationRequested)
        {
            Location loc;
            try
            {
                loc = await locationSource.ReadAsync(ct);
            }
            catch (OperationCanceledException)
            {
                RemoveProcess(processId, locationSource);
                yield break;
            }

            logger.LogInformation("Process (pid: {processId}) gets location: {location}", processId, loc);
            yield return loc;
        }

        RemoveProcess(processId, locationSource);

        logger.LogInformation("Server: done!");
    }

    private void RemoveProcess(int processId, LocationSource locationSource)
    {
        logger.LogInformation("Client disconnected (pid: {processId})", processId);
        synchronizedScrollingRepository.InformSubscribers(processId, new Location { MessageType = MessageType.Unsubscribe }, locationSource);
        synchronizedScrollingRepository.Delete(processId);
    }
}

