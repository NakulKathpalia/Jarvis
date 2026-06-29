namespace Jarvis.Core.Agents.Coding.MultiAgent;

/// <summary>
/// Represents a role task in the coding orchestrator.
/// </summary>
public sealed class CodingAgentTask
{
    /// <summary>
    /// Gets or sets the role.
    /// </summary>
    public CodingAgentRole Role { get; set; }

    /// <summary>
    /// Gets or sets task description.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}
