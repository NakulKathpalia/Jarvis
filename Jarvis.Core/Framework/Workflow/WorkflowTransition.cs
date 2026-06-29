namespace Jarvis.Core.Framework.Workflow;

/// <summary>
/// Represents a transition between workflow lifecycle states.
/// </summary>
public sealed class WorkflowTransition
{
    /// <summary>
    /// Gets or sets the state before the transition.
    /// </summary>
    public WorkflowState From { get; set; }

    /// <summary>
    /// Gets or sets the state after the transition.
    /// </summary>
    public WorkflowState To { get; set; }

    /// <summary>
    /// Gets or sets the transition reason.
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the transition time in UTC.
    /// </summary>
    public DateTimeOffset TimestampUtc { get; set; } = DateTimeOffset.UtcNow;
}
