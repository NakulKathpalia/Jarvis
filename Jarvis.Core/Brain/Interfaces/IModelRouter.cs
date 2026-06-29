namespace Jarvis.Core.Brain.Interfaces;

using Jarvis.Core.Brain.Models;

/// <summary>
/// Defines model routing before execution.
/// </summary>
public interface IModelRouter
{
    /// <summary>
    /// Adds model routing information to the decision.
    /// </summary>
    /// <param name="decision">The current routing decision.</param>
    /// <param name="intent">The detected intent.</param>
    /// <returns>The updated routing decision.</returns>
    RoutingDecision Route(RoutingDecision decision, IntentResult intent);
}
