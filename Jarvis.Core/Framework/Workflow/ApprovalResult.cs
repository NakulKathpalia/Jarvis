namespace Jarvis.Core.Framework.Workflow;

/// <summary>
/// Represents the result of a workflow approval request.
/// </summary>
public sealed class ApprovalResult
{
    /// <summary>
    /// Gets or sets the approval request identifier.
    /// </summary>
    public string RequestId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the request was approved.
    /// </summary>
    public bool Approved { get; set; }

    /// <summary>
    /// Gets or sets the optional reason supplied with the result.
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}
