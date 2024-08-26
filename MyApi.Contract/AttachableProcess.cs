namespace MyApi.Contract;
using ProtoBuf;

/// <summary>
/// Process information.
/// </summary>
[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public sealed record AttachableProcess
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AttachableProcess"/> class.
    /// </summary>
    public AttachableProcess()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AttachableProcess"/> class.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="isEnabled">Can be attached to.</param>
    public AttachableProcess(string filePath, bool isEnabled)
    {
        FilePath = filePath;
        IsEnabled = isEnabled;
    }

    /// <summary>
    /// Gets the file path.
    /// </summary>
    public string FilePath { get; init; } = null!;

    /// <summary>
    /// Gets a value indicating whether the process is available for attachment.
    /// True is not attached to another process.
    /// </summary>
    public bool IsEnabled { get; init; }
}
