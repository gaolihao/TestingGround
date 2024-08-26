namespace MyApi.Contract;
using ProtoBuf.Grpc;
using Grpc.Core;
using ProtoBuf;
using ProtoBuf.Grpc.Configuration;


/// <summary>
/// Interface for synchronized scrolling service.
/// </summary>
[Service]
public interface ISynchronizedFeatureListService
{
    /// <summary>
    /// Connects NoviView to Instance Manager.
    /// </summary>
    /// <param name="locations">Stream of locations.</param>
    /// <param name="context">Call context.</param>
    /// <remarks>
    /// Use the following headers to pass key information
    /// <see cref="BaseMessageHeader.HeaderProcessId"></see> header to set the calling process id. Must be set as bytes using <see cref="BitConverter.GetBytes(int)"/>.
    /// <see cref="ConnectMessageHeader.HeaderFilePath"></see> header to set the file name. File name should be encoded with <see cref="System.Net.WebUtility.UrlEncode(string?)"/>.
    /// </remarks>
    /// <returns>Stream of modified locations.</returns>
    [Operation]
    IAsyncEnumerable<FeatureList> ConnectAsync(IAsyncEnumerable<FeatureList> locations, CallContext context = default);

    /// <summary>
    /// Gets instances to subscribe to.
    /// </summary>
    /// <param name="context">Call context.</param>
    /// <returns>IDictionary of processes and its metadata.</returns>
    [Operation]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "PBN2008:ServiceContractAnalyzer.PossiblyNotSerializable", Justification = "IDictionary seems to work fine")]
    Task<IDictionary<int, AttachableProcess>> GetProcessesAsync(CallContext context = default);

    /// <summary>
    /// Subscribes to another instance.
    /// </summary>
    /// <param name="subscriptionRequest">subscription request with process id to connect to.</param>
    /// <param name="context">Call context.</param>
    /// <returns>Task to complete.</returns>
    [Operation]
    Task SubscribeAsync(SubscriptionRequest subscriptionRequest, CallContext context = default);
}
