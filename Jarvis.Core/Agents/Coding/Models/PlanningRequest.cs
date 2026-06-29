namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents a request to build a read-only coding plan.
/// </summary>
public sealed class PlanningRequest
{
    /// <summary>
    /// Gets or sets the coding request text.
    /// </summary>
    public string RequestText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the selected context package.
    /// </summary>
    public ContextPackage ContextPackage { get; set; } = new();
}
