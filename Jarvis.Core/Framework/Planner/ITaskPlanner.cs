namespace Jarvis.Core.Framework.Planner;

using Jarvis.Core.Framework.Models;

/// <summary>
/// Defines the planner responsible for selecting an agent for a task request.
/// </summary>
public interface ITaskPlanner
{
    /// <summary>
    /// Selects the agent name that should execute the request.
    /// </summary>
    /// <param name="request">The incoming task request.</param>
    /// <param name="context">The execution context for the request.</param>
    /// <param name="cancellationToken">A token that cancels planning.</param>
    /// <returns>The selected agent name.</returns>
    Task<string> SelectAgentAsync(
        TaskRequest request,
        ExecutionContext context,
        CancellationToken cancellationToken = default);
}
