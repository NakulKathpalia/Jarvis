namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents a validated patch plan.
/// </summary>
public sealed class PatchPlan
{
    /// <summary>
    /// Gets or sets the plan identifier.
    /// </summary>
    public string PlanId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets or sets a value indicating whether the plan is a dry run.
    /// </summary>
    public bool DryRun { get; set; }

    /// <summary>
    /// Gets or sets validated patch operations.
    /// </summary>
    public List<PatchOperation> Operations { get; set; } = [];
}
