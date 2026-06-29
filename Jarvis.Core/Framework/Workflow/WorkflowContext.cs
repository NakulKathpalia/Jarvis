namespace Jarvis.Core.Framework.Workflow;

using Jarvis.Core.Brain.Models;
using Jarvis.Core.Core.Logging;

/// <summary>
/// Contains generic runtime information for a workflow execution.
/// </summary>
public sealed class WorkflowContext
{
    /// <summary>
    /// Gets or sets the workflow identifier.
    /// </summary>
    public string WorkflowId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the execution plan being run.
    /// </summary>
    public ExecutionPlan ExecutionPlan { get; set; } = new();

    /// <summary>
    /// Gets or sets the cancellation token for the workflow.
    /// </summary>
    public CancellationToken CancellationToken { get; set; }

    /// <summary>
    /// Gets or sets shared workflow variables.
    /// </summary>
    public Dictionary<string, object?> Variables { get; set; } = [];

    /// <summary>
    /// Gets or sets the optional framework logger.
    /// </summary>
    public IFrameworkLogger? Logger { get; set; }

    /// <summary>
    /// Gets or sets generic workflow metrics.
    /// </summary>
    public Dictionary<string, double> Metrics { get; set; } = [];

    /// <summary>
    /// Gets or sets the context creation time in UTC.
    /// </summary>
    public DateTimeOffset TimestampUtc { get; set; } = DateTimeOffset.UtcNow;
}
