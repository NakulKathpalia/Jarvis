namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents a read-only coding plan.
/// </summary>
public sealed class CodingPlan
{
    /// <summary>
    /// Gets or sets the plan identifier.
    /// </summary>
    public string PlanId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets or sets the original request text.
    /// </summary>
    public string RequestText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the plan steps.
    /// </summary>
    public List<CodingStep> Steps { get; set; } = [];
}
