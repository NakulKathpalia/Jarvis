namespace Jarvis.Core.Brain.Models;

/// <summary>
/// Represents a generic analysis of a task request.
/// </summary>
public sealed class TaskAnalysis
{
    /// <summary>
    /// Gets or sets the estimated complexity.
    /// </summary>
    public string Complexity { get; set; } = "Low";

    /// <summary>
    /// Gets or sets a value indicating whether the task has multiple steps.
    /// </summary>
    public bool IsMultiStep { get; set; }

    /// <summary>
    /// Gets or sets the analyzed execution steps.
    /// </summary>
    public List<ExecutionStep> Steps { get; set; } = [];
}
