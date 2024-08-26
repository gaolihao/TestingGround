using System.Collections.Concurrent;
using static Google.Protobuf.Reflection.SourceCodeInfo.Types;
using MyApi.Contract;

namespace MyApi;
public class SynchronizedFeatureListRepository
{
    private readonly ConcurrentDictionary<int, FeatureSource> featureSources = new();

    // Note: Add logger later
    public SynchronizedFeatureListRepository() 
    { 
    
    }

    // Get all the processes that can be connected to
    public IDictionary<int, FeatureSource> All => featureSources;

    // Get or add a process by id
    public FeatureSource GetPathAndQueue(int processId)
    {
        return featureSources.GetOrAdd(processId, (processId) => new FeatureSource());
    }

    // Delete process
    public void Delete(int processId)
    {
        featureSources.Remove(processId, out var _);
        foreach (var featureSource in featureSources.Values)
        {
            featureSource.Subscribers.RemoveWhere(subscriber => subscriber == processId);
        }
    }

    /// <summary>
    /// Unsibscribes a child process from the parent.
    /// </summary>
    /// <param name="processId">Process id to unsubscribe.</param>
    internal void Unsubscribe(int processId)
    {
        foreach (var pair in featureSources)
        {
            pair.Value.Subscribers.Remove(processId);
        }
    }

    /// <summary>
    /// Informs parent that the child location has changed.
    /// </summary>
    /// <param name="processId">Process id of the child process.</param>
    /// <param name="location">New location.</param>
    internal void InformParent(int processId, FeatureList featureList)
    {
        FeatureSource? parent = null;
        foreach (var pair in featureSources)
        {
            if (pair.Value.Subscribers.Contains(processId))
            {
                //logger.LogDebug("Send location {location} to {processId}", location, processId);
                parent = pair.Value;
                break;
            }
        }

        if (parent is null)
        {
            return;
        }

        parent.Publish(featureList);
        InformSubscribers(processId, featureList, parent);
    }

    /// <summary>
    /// Informs subscribers that the location has changed.
    /// </summary>
    /// <param name="triggerProcessId">ID of procecess which triggered update. Can be either parent or one of subscribers.</param>
    /// <param name="location">Location to report.</param>
    /// <param name="locationSource">the location source.</param>
    internal void InformSubscribers(int triggerProcessId, FeatureList featureList, FeatureSource featureSource)
    {
        var recipients = new List<int>();
        foreach (var subscriber in featureSource.Subscribers)
        {
            if (subscriber == triggerProcessId)
            {
                continue;
            }

            recipients.Add(subscriber);
            var subscriberPq = GetPathAndQueue(subscriber);
            subscriberPq.Publish(featureList);
        }

        //logger.LogInformation("Process (pid: {processId}) reports {location} to {recipients}", triggerProcessId, location, recipients);
    }

    /// <summary>
    /// Check if a processId is already a subscriber.
    /// </summary>
    /// <param name="processId">Process id to check.</param>
    /// <returns>True if process id is a subscriber; otherwise false.</returns>
    internal bool IsSubscriber(int processId)
    { 
        return featureSources.Values.Any(z => z.Subscribers.Contains(processId)); 
    }
}
