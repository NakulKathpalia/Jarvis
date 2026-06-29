namespace Jarvis.Core.Agents.Coding.MultiAgent;

using Jarvis.Core.Agents.Coding.Context.Intelligent;

/// <summary>
/// Produces a planning role result.
/// </summary>
public sealed class PlannerRole
{
    /// <summary>
    /// Executes the planner role.
    /// </summary>
    public CodingAgentResult Execute(IntelligentContextResult context)
    {
        return new CodingAgentResult
        {
            Role = CodingAgentRole.Planner,
            Succeeded = context.Succeeded,
            Output = $"Selected {context.SelectedFiles.Count} files and {context.SelectedSymbols.Count} symbols."
        };
    }
}
