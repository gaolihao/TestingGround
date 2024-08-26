namespace MyApi.Contract;
using ProtoBuf;

/// <summary>
/// Subscription Request.
/// </summary>
[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public record SubscriptionRequest
{
    /// <summary>
    /// gets or sets Process if to connect to.
    /// </summary>
    public int SubscribeToProcessId { get; set; }
}
