namespace Jarvis.Core.Core.Exceptions;

/// <summary>
/// Represents a failure to resolve a planned agent.
/// </summary>
public sealed class AgentNotFoundException : FrameworkException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AgentNotFoundException"/> class.
    /// </summary>
    /// <param name="agentName">The unresolved agent name.</param>
    public AgentNotFoundException(string agentName)
        : base($"Agent was not found: {agentName}")
    {
        AgentName = agentName;
    }

    /// <summary>
    /// Gets the unresolved agent name.
    /// </summary>
    public string AgentName { get; }
}
