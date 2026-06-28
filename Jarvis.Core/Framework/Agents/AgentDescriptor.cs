namespace Jarvis.Core.Framework.Agents;

/// <summary>
/// Describes metadata for a Jarvis agent.
/// </summary>
public sealed class AgentDescriptor
{
    /// <summary>
    /// Gets or sets the unique agent name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name shown to humans.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the agent description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the agent version.
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Gets or sets the task types supported by the agent.
    /// </summary>
    public IReadOnlyCollection<string> SupportedTaskTypes { get; set; } = [];
}
