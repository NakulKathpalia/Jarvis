namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents one read-only coding plan step.
/// </summary>
public sealed class CodingStep
{
    /// <summary>
    /// Gets or sets the step order.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets the step title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the planning strategy.
    /// </summary>
    public PlanningStrategy Strategy { get; set; }

    /// <summary>
    /// Gets or sets factual target files.
    /// </summary>
    public List<string> TargetFiles { get; set; } = [];

    /// <summary>
    /// Gets or sets factual target symbols.
    /// </summary>
    public List<string> TargetSymbols { get; set; } = [];
}
