namespace Jarvis.Core.Framework.Workflow;

using Jarvis.Core.Framework.Models;

/// <summary>
/// Represents the result of workflow execution.
/// </summary>
public sealed class WorkflowResult
{
    /// <summary>
    /// Gets or sets the workflow identifier.
    /// </summary>
    public string WorkflowId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the workflow succeeded.
    /// </summary>
    public bool Succeeded { get; set; }

    /// <summary>
    /// Gets or sets the final workflow state.
    /// </summary>
    public WorkflowState State { get; set; } = WorkflowState.Created;

    /// <summary>
    /// Gets or sets the workflow start time in UTC.
    /// </summary>
    public DateTimeOffset? StartedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the workflow finish time in UTC.
    /// </summary>
    public DateTimeOffset? FinishedAtUtc { get; set; }

    /// <summary>
    /// Gets the workflow duration when start and finish times are available.
    /// </summary>
    public TimeSpan? Duration => FinishedAtUtc - StartedAtUtc;

    /// <summary>
    /// Gets or sets framework task results produced by workflow steps.
    /// </summary>
    public List<TaskResult> StepResults { get; set; } = [];

    /// <summary>
    /// Gets or sets workflow errors.
    /// </summary>
    public List<string> Errors { get; set; } = [];

    /// <summary>
    /// Gets or sets workflow outputs by step identifier.
    /// </summary>
    public Dictionary<string, object?> Outputs { get; set; } = [];

    /// <summary>
    /// Gets or sets the in-memory workflow history.
    /// </summary>
    public WorkflowHistory History { get; set; } = new();
}
