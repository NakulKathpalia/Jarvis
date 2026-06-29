namespace Jarvis.Core.Framework.Workflow;

/// <summary>
/// Represents read-only workflow engine metrics.
/// </summary>
public sealed class WorkflowMetrics
{
    /// <summary>
    /// Gets or sets the total workflow execution time.
    /// </summary>
    public TimeSpan ExecutionTime { get; set; }

    /// <summary>
    /// Gets or sets the number of retries.
    /// </summary>
    public int Retries { get; set; }

    /// <summary>
    /// Gets or sets the number of steps executed in parallel batches.
    /// </summary>
    public int ParallelSteps { get; set; }

    /// <summary>
    /// Gets or sets the number of failures.
    /// </summary>
    public int Failures { get; set; }

    /// <summary>
    /// Gets or sets the completion rate from zero to one.
    /// </summary>
    public double CompletionRate { get; set; }

    /// <summary>
    /// Gets or sets the average workflow duration.
    /// </summary>
    public TimeSpan AverageDuration { get; set; }
}
