namespace Jarvis.Core.Framework.Registry;

using Jarvis.Core.Framework.Agents;

/// <summary>
/// Provides an in-memory registry for framework agents.
/// </summary>
public sealed class AgentRegistry : IAgentRegistry
{
    private readonly Dictionary<string, IAgent> agents = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public void Register(IAgent agent)
    {
        ArgumentNullException.ThrowIfNull(agent);

        if (string.IsNullOrWhiteSpace(agent.Name))
        {
            throw new ArgumentException("Agent name is required.", nameof(agent));
        }

        agents[agent.Name] = agent;
    }

    /// <inheritdoc />
    public bool Remove(string name)
    {
        return !string.IsNullOrWhiteSpace(name) && agents.Remove(name);
    }

    /// <inheritdoc />
    public IAgent? GetByName(string name)
    {
        return string.IsNullOrWhiteSpace(name) ? null : agents.GetValueOrDefault(name);
    }

    /// <inheritdoc />
    public IReadOnlyCollection<IAgent> List()
    {
        return agents.Values.ToArray();
    }
}
