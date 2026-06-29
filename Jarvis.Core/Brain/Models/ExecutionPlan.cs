namespace Jarvis.Core.Brain.Models;

/// <summary>
/// Represents a non-executing plan produced by the Brain.
/// </summary>
public sealed class ExecutionPlan
{
    /// <summary>
    /// Gets or sets the plan identifier.
    /// </summary>
    public string PlanId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets or sets the original request text.
    /// </summary>
    public string Input { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the detected intent.
    /// </summary>
    public IntentResult Intent { get; set; } = new();

    /// <summary>
    /// Gets or sets the task analysis.
    /// </summary>
    public TaskAnalysis Analysis { get; set; } = new();

    /// <summary>
    /// Gets or sets the routing decision.
    /// </summary>
    public RoutingDecision Routing { get; set; } = new();

    /// <summary>
    /// Gets or sets the planned execution steps.
    /// </summary>
    public List<ExecutionStep> Steps { get; set; } = [];

    /// <summary>
    /// Gets or sets the plan creation time in UTC.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
