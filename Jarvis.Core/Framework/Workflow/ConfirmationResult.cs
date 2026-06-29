namespace Jarvis.Core.Framework.Workflow;

/// <summary>
/// Represents the result of a workflow confirmation request.
/// </summary>
public sealed class ConfirmationResult
{
    /// <summary>
    /// Gets or sets the confirmation request identifier.
    /// </summary>
    public string RequestId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the request was confirmed.
    /// </summary>
    public bool Confirmed { get; set; }

    /// <summary>
    /// Gets or sets the optional reason supplied with the result.
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}
