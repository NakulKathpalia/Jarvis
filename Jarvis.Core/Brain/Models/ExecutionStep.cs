namespace Jarvis.Core.Brain.Models;

/// <summary>
/// Represents one generic step in an execution plan.
/// </summary>
public sealed class ExecutionStep
{
    /// <summary>
    /// Gets or sets the step identifier.
    /// </summary>
    public string StepId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets or sets the step order.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets the generic step input.
    /// </summary>
    public string Input { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the generic step type.
    /// </summary>
    public string StepType { get; set; } = string.Empty;
}
