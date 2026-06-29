namespace Jarvis.Core.Framework.Workflow;

/// <summary>
/// Represents a workflow approval request for sensitive actions.
/// </summary>
public sealed class ApprovalRequest
{
    /// <summary>
    /// Gets or sets the request identifier.
    /// </summary>
    public string RequestId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets or sets the workflow identifier.
    /// </summary>
    public string WorkflowId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the step identifier.
    /// </summary>
    public string StepId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the action that requires approval.
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the approval message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
