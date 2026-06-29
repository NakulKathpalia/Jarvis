namespace Jarvis.Core.Brain.Models;

/// <summary>
/// Represents the result of workflow planning.
/// </summary>
public sealed class WorkflowResult
{
    /// <summary>
    /// Gets or sets a value indicating whether workflow planning succeeded.
    /// </summary>
    public bool Succeeded { get; set; }

    /// <summary>
    /// Gets or sets the execution plan.
    /// </summary>
    public ExecutionPlan Plan { get; set; } = new();

    /// <summary>
    /// Gets or sets the workflow planning message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
