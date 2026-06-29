namespace Jarvis.Core.Brain.Models;

/// <summary>
/// Represents generic routing choices made before execution.
/// </summary>
public sealed class RoutingDecision
{
    /// <summary>
    /// Gets or sets the selected agent name.
    /// </summary>
    public string AgentName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the selected skill name.
    /// </summary>
    public string SkillName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the selected tool name.
    /// </summary>
    public string ToolName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the selected model route.
    /// </summary>
    public string ModelRoute { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the decision reason.
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}
