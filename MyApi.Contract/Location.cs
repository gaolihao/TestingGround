namespace MyApi.Contract;
using ProtoBuf;

/// <summary>
/// Scrollbar location.
/// </summary>
[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public sealed record Location
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Location"/> class.
    /// </summary>
    public Location()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Location"/> class.
    /// </summary>
    /// <param name="weldIdFine">Weld Id correlated to scroll bar location. Fractions are deterined by distance.</param>
    public Location(long weldIdFine)
    {
        WeldIdFine = weldIdFine;
    }

    /// <summary>
    /// Gets the Weld Id correlated to scroll bar location. Fractions are deterined by distance.
    /// </summary>
    public long WeldIdFine { get; init; }

    /// <summary>
    /// Gets the message type.
    /// </summary>
    public MessageType MessageType { get; init; }

    /// <summary>
    /// Converts long to location.
    /// </summary>
    /// <param name="weldIdFine">Fine Weld Id.</param>
    public static implicit operator Location(long weldIdFine) => new(weldIdFine);

    /// <inheritdoc/>
    public override string ToString()
        => MessageType == MessageType.Unsubscribe ? "Unsubscribe" : $"{WeldIdFine / 100000d}";
}
