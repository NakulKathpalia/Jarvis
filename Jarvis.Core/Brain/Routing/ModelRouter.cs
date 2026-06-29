namespace Jarvis.Core.Brain.Routing;

using Jarvis.Core.Brain.Interfaces;
using Jarvis.Core.Brain.Models;

/// <summary>
/// Provides placeholder model routing for future model integrations.
/// </summary>
public sealed class ModelRouter : IModelRouter
{
    /// <inheritdoc />
    public RoutingDecision Route(RoutingDecision decision, IntentResult intent)
    {
        ArgumentNullException.ThrowIfNull(decision);

        decision.ModelRoute = "None";
        return decision;
    }
}
