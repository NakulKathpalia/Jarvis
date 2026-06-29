namespace Jarvis.Core.Brain.Routing;

using Jarvis.Core.Brain.Interfaces;
using Jarvis.Core.Brain.Models;
using Jarvis.Core.Framework.Registry;

/// <summary>
/// Selects an agent from the framework registry.
/// </summary>
public sealed class AgentSelector : IAgentSelector
{
    /// <inheritdoc />
    public RoutingDecision SelectAgent(IntentResult intent, IAgentRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(intent);
        ArgumentNullException.ThrowIfNull(registry);

        var candidates = BuildCandidateNames(intent.TaskType);
        var agent = candidates
            .Select(registry.GetByName)
            .FirstOrDefault(candidate => candidate is not null);

        return new RoutingDecision
        {
            AgentName = agent?.Name ?? string.Empty,
            Reason = agent is null
                ? "No registered agent matched the detected intent."
                : "Selected registered agent from detected intent."
        };
    }

    private static IReadOnlyCollection<string> BuildCandidateNames(string taskType)
    {
        if (string.IsNullOrWhiteSpace(taskType))
        {
            return [];
        }

        return
        [
            $"{taskType}Agent",
            $"{taskType} Agent",
            taskType
        ];
    }
}
