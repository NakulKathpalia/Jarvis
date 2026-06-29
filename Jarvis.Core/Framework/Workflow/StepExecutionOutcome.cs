namespace Jarvis.Core.Framework.Workflow;

using Jarvis.Core.Framework.Models;

/// <summary>
/// Represents the outcome of executing a workflow step.
/// </summary>
public sealed class StepExecutionOutcome
{
    /// <summary>
    /// Gets or sets the workflow step.
    /// </summary>
    public WorkflowStep Step { get; set; } = new();

    /// <summary>
    /// Gets or sets the framework task result.
    /// </summary>
    public TaskResult Result { get; set; } = new();

    /// <summary>
    /// Gets or sets history entries produced during step execution.
    /// </summary>
    public List<string> HistoryEntries { get; set; } = [];
}
