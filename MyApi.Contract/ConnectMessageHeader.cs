namespace MyApi.Contract;
using ProtoBuf.Grpc;
using Grpc.Core;
using System.Net;

/// <summary>
/// Initializes a new instance of the <see cref="ConnectMessageHeader"/> class.
/// </summary>
/// <param name="FilePath">The file path of the caller.</param>
/// <param name="InitialLocation">The initial location of the caller.</param>
public record ConnectMessageHeader(string FilePath, FeatureList InitialLocation) : BaseMessageHeader
{
    /// <summary>
    /// Initial Location Header.
    /// </summary>
    public const string HeaderInitialLocation = "InitialLocation-bin";

    /// <summary>
    /// FilePath header.
    /// </summary>
    public const string HeaderFilePath = "FilePath";

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectMessageHeader"/> class.
    /// </summary>
    /// <param name="processId">Process ID of the caller.</param>
    /// <param name="filePath">The file path of the caller.</param>
    /// <param name="initialLocation">The initial location of the caller.</param>
    public ConnectMessageHeader(int processId, string filePath, FeatureList initialLocation)
        : this(filePath, initialLocation)
    {
        ProcessId = processId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectMessageHeader"/> class.
    /// </summary>
    /// <param name="filePath">The file path of the caller.</param>
    public ConnectMessageHeader(string filePath)
        : this(filePath, new FeatureList())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectMessageHeader"/> class.
    /// </summary>
    /// <param name="context">gRPC call context.</param>
    /// <exception cref="RpcException">Throws if couldn't convert.</exception>
    public ConnectMessageHeader(CallContext context)
    : this(
          GetProcessId(context),
          WebUtility.UrlDecode(context.RequestHeaders.GetString(HeaderFilePath)
            ?? throw new RpcException(Status.DefaultCancelled, "filePath needs to be set")),
          new FeatureList(BitConverter.ToInt64(context.RequestHeaders.GetBytes(HeaderInitialLocation)
            ?? throw new RpcException(Status.DefaultCancelled, "need initial location"))))
    {
    }

    /// <summary>
    /// Deconstructs an instance of the <see cref="ConnectMessageHeader"/> class.
    /// </summary>
    /// <param name="processId">Process ID of the caller.</param>
    /// <param name="filePath">The file path of the caller.</param>
    /// <param name="initialLocation">The initial location of the caller.</param>
    public void Deconstruct(out int processId, out string filePath, out FeatureList initialLocation)
    {
        processId = ProcessId;
        filePath = FilePath;
        initialLocation = InitialLocation;
    }

    private protected override IList<Metadata.Entry> GetHeaders()
    {
        var baseList = base.GetHeaders();
        baseList.Add(new Metadata.Entry(HeaderFilePath, WebUtility.UrlEncode(FilePath)));
        baseList.Add(new Metadata.Entry(HeaderInitialLocation, BitConverter.GetBytes(InitialLocation.WeldIdFine)));
        return baseList;
    }
}
