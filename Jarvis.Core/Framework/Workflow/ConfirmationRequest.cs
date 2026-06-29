namespace Jarvis.Core.Framework.Workflow;

/// <summary>
/// Represents a workflow confirmation request.
/// </summary>
public sealed class ConfirmationRequest
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
    /// Gets or sets the confirmation message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
