namespace Jarvis.Core.Brain.Interfaces;

using Jarvis.Core.Brain.Models;
using Jarvis.Core.Framework.Registry;

/// <summary>
/// Defines agent selection for a detected intent.
/// </summary>
public interface IAgentSelector
{
    /// <summary>
    /// Selects an agent using the framework registry.
    /// </summary>
    /// <param name="intent">The detected intent.</param>
    /// <param name="registry">The agent registry.</param>
    /// <returns>The routing decision.</returns>
    RoutingDecision SelectAgent(IntentResult intent, IAgentRegistry registry);
}
