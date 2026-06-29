namespace Jarvis.Core.Brain.Routing;

using Jarvis.Core.Brain.Interfaces;
using Jarvis.Core.Brain.Models;
using Jarvis.Core.Shared.Interfaces;

/// <summary>
/// Selects a tool from supplied candidates.
/// </summary>
public sealed class ToolSelector : IToolSelector
{
    /// <inheritdoc />
    public RoutingDecision SelectTool(RoutingDecision decision, IReadOnlyCollection<ITool> tools)
    {
        ArgumentNullException.ThrowIfNull(decision);

        var tool = tools.FirstOrDefault();
        decision.ToolName = tool?.Name ?? string.Empty;
        return decision;
    }
}
