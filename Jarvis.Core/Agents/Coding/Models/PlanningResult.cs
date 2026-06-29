namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents the result of read-only coding planning.
/// </summary>
public sealed class PlanningResult
{
    /// <summary>
    /// Gets or sets a value indicating whether planning succeeded.
    /// </summary>
    public bool Succeeded { get; set; }

    /// <summary>
    /// Gets or sets the generated coding plan.
    /// </summary>
    public CodingPlan Plan { get; set; } = new();

    /// <summary>
    /// Gets or sets validation messages.
    /// </summary>
    public List<string> Messages { get; set; } = [];
}
