using ProtoBuf.Grpc;

namespace MyApi.Contract;

public record ConnectMessageHeaderFeatureList( ) : BaseMessageHeader
{
    public ConnectMessageHeaderFeatureList(int processId)
        : this()
    {
        ProcessId = processId;
    }

    public ConnectMessageHeaderFeatureList(CallContext context)
    : this(
          GetProcessId(context)
          )
    {
    }
}
