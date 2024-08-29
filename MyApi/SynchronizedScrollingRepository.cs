namespace MyApi;

using MyApi.Contract;
using System.Collections.Concurrent;

/// <summary>
/// Repository containing all instance data.
/// </summary>
public class SynchronizedScrollingRepository
{
    private readonly ILogger<SynchronizedScrollingRepository> logger;

    private readonly ConcurrentDictionary<int, LocationSource> locationSources = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SynchronizedScrollingRepository"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger for this class.</param>
    public SynchronizedScrollingRepository(ILoggerFactory loggerFactory)
    {
        logger = loggerFactory.CreateLogger<SynchronizedScrollingRepository>();
    }

    /// <summary>
    /// Gets all the processes that can be connected to.
    /// </summary>
    public IDictionary<int, LocationSource> All => locationSources;

    /// <summary>
    /// Gets a process by id.
    /// </summary>
    /// <param name="processId">Process Id.</param>
    /// <returns>Location source.</returns>
    public LocationSource GetPathAndQueue(int processId)
        => locationSources[processId];

    /// <summary>
    /// Gets or adds a process.
    /// </summary>
    /// <param name="processId">Process Id.</param>
    /// <param name="filePath">File path of the open document.</param>
    /// <returns>Location source.</returns>
    public LocationSource GetPathAndQueue(int processId, string filePath)
    {
        return locationSources.GetOrAdd(processId, (processId) => new LocationSource(filePath));
    }

    /// <summary>
    /// Deletes process, because the client switched to another feature set or closed.
    /// </summary>
    /// <param name="processId">Process Id.</param>
    public void Delete(int processId)
    {
        locationSources.Remove(processId, out var _);
        foreach (var locationSource in locationSources.Values)
        {
            locationSource.Subscribers.RemoveWhere(subscriber => subscriber == processId);
        }
    }

    /// <summary>
    /// Unsibscribes a child process from the parent.
    /// </summary>
    /// <param name="processId">Process id to unsubscribe.</param>
    internal void Unsubscribe(int processId)
    {
        foreach (var pair in locationSources)
        {
            pair.Value.Subscribers.Remove(processId);
        }
    }

    /// <summary>
    /// Informs parent that the child location has changed.
    /// </summary>
    /// <param name="processId">Process id of the child process.</param>
    /// <param name="location">New location.</param>
    internal void InformParent(int processId, Location location)
    {
        LocationSource? parent = null;
        foreach (var pair in locationSources)
        {
            if (pair.Value.Subscribers.Contains(processId))
            {
                logger.LogDebug("Send location {location} to {processId}", location, processId);
                parent = pair.Value;
                break;
            }
        }

        if (parent is null)
        {
            return;
        }

        parent.Publish(location);
        InformSubscribers(processId, location, parent);
    }

    /// <summary>
    /// Informs subscribers that the location has changed.
    /// </summary>
    /// <param name="triggerProcessId">ID of procecess which triggered update. Can be either parent or one of subscribers.</param>
    /// <param name="location">Location to report.</param>
    /// <param name="locationSource">the location source.</param>
    internal void InformSubscribers(int triggerProcessId, Location location, LocationSource locationSource)
    {
        var recipients = new List<int>();
        foreach (var subscriber in locationSource.Subscribers)
        {
            if (subscriber == triggerProcessId)
            {
                continue;
            }

            recipients.Add(subscriber);
            var subscriberPq = GetPathAndQueue(subscriber);
            subscriberPq.Publish(location);
        }

        logger.LogInformation("Process (pid: {processId}) reports {location} to {recipients}", triggerProcessId, location, recipients);
    }

    /// <summary>
    /// Check if a processId is already a subscriber.
    /// </summary>
    /// <param name="processId">Process id to check.</param>
    /// <returns>True if process id is a subscriber; otherwise false.</returns>
    internal bool IsSubscriber(int processId)
        => locationSources.Values.Any(z => z.Subscribers.Contains(processId));
}
