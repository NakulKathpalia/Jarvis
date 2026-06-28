namespace Jarvis.Core.Framework.Registry;

using Jarvis.Core.Framework.Agents;

/// <summary>
/// Defines a registry for resolving Jarvis agents by name.
/// </summary>
public interface IAgentRegistry
{
    /// <summary>
    /// Registers an agent by its unique name.
    /// </summary>
    /// <param name="agent">The agent to register.</param>
    void Register(IAgent agent);

    /// <summary>
    /// Removes a registered agent.
    /// </summary>
    /// <param name="name">The agent name.</param>
    /// <returns><c>true</c> when an agent was removed.</returns>
    bool Remove(string name);

    /// <summary>
    /// Gets a registered agent by name.
    /// </summary>
    /// <param name="name">The agent name.</param>
    /// <returns>The agent when registered; otherwise <c>null</c>.</returns>
    IAgent? GetByName(string name);

    /// <summary>
    /// Lists all registered agents.
    /// </summary>
    /// <returns>The registered agents.</returns>
    IReadOnlyCollection<IAgent> List();
}
