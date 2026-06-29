namespace Jarvis.Core.Framework.Workflow;

/// <summary>
/// Represents a point-in-time workflow progress snapshot.
/// </summary>
public sealed class WorkflowProgress
{
    /// <summary>
    /// Gets or sets the total number of workflow steps.
    /// </summary>
    public int TotalSteps { get; set; }

    /// <summary>
    /// Gets or sets the number of completed workflow steps.
    /// </summary>
    public int CompletedSteps { get; set; }

    /// <summary>
    /// Gets or sets the current running step identifier.
    /// </summary>
    public string RunningStep { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets all currently running step identifiers.
    /// </summary>
    public List<string> RunningSteps { get; set; } = [];

    /// <summary>
    /// Gets or sets the failed step identifier.
    /// </summary>
    public string FailedStep { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the progress percentage.
    /// </summary>
    public double ProgressPercent { get; set; }

    /// <summary>
    /// Gets or sets the estimated remaining step count.
    /// </summary>
    public int EstimatedRemainingSteps { get; set; }
}
