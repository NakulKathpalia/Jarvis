namespace Jarvis.Core.Brain.Interfaces;

using Jarvis.Core.Brain.Models;
using Jarvis.Core.Shared.Interfaces;

/// <summary>
/// Defines tool selection for a routing decision.
/// </summary>
public interface IToolSelector
{
    /// <summary>
    /// Selects a compatible tool from supplied candidates.
    /// </summary>
    /// <param name="decision">The current routing decision.</param>
    /// <param name="tools">The candidate tools.</param>
    /// <returns>The updated routing decision.</returns>
    RoutingDecision SelectTool(RoutingDecision decision, IReadOnlyCollection<ITool> tools);
}
