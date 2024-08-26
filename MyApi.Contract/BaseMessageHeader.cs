namespace MyApi.Contract;
using ProtoBuf.Grpc;
using Grpc.Core;

/// <summary>
/// Initializes a new instance of the <see cref="BaseMessageHeader"/> class.
/// </summary>
/// <param name="ProcessId">Process ID of the caller.</param>
public record BaseMessageHeader(int ProcessId)
{
    /// <summary>
    /// Process Id Header.
    /// </summary>
    public const string HeaderProcessId = "ProcessId-bin";

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseMessageHeader"/> class.
    /// </summary>
    public BaseMessageHeader()
        : this(Environment.ProcessId)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseMessageHeader"/> class.
    /// </summary>
    /// <param name="context">gRPC call context.</param>
    public BaseMessageHeader(CallContext context)
    : this(GetProcessId(context))
    {
    }

    /// <summary>
    /// Create <see cref="CallOptions"/> object.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Resulting CallOptions object.</returns>
    public CallOptions ToCallOptions(CancellationToken ct = default)
    {
        var reqHeaders = new Metadata();
        foreach (var entry in GetHeaders())
        {
            reqHeaders.Add(entry);
        }

        var options = new CallOptions(cancellationToken: ct, headers: reqHeaders);
        return options;
    }

    private protected static int GetProcessId(CallContext context)
        => BitConverter.ToInt32(context.RequestHeaders.GetBytes(HeaderProcessId)
            ?? throw new RpcException(Status.DefaultCancelled, "process needs to be a valid integer"));

    private protected virtual IList<Metadata.Entry> GetHeaders()
    {
        return
        [
            new Metadata.Entry(HeaderProcessId, BitConverter.GetBytes(ProcessId)),
        ];
    }
}
