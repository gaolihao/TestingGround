namespace MyApi.Contract;
using ProtoBuf;

/// <summary>
/// Message type enumeration.
/// </summary>
[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public enum MessageType
{
    /// <summary>
    /// Location message.
    /// </summary>
    Location,

    /// <summary>
    /// unsubscribe message.
    /// </summary>
    Unsubscribe,
}

