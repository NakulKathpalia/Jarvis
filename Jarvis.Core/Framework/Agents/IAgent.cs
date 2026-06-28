namespace Jarvis.Core.Framework.Agents;

using Jarvis.Core.Framework.Context;
using Jarvis.Core.Framework.Models;
using Jarvis.Core.Framework.Routing;

/// <summary>
/// Defines a Jarvis agent that can execute a planned task through skills and tools.
/// </summary>
public interface IAgent
{
    /// <summary>
    /// Gets the unique agent name used by the registry and planner.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Executes the agent for the supplied context.
    /// </summary>
    /// <param name="context">The agent execution context.</param>
    /// <param name="toolExecutor">The tool executor available to the agent.</param>
    /// <param name="cancellationToken">A token that cancels execution.</param>
    /// <returns>The task result produced by the agent.</returns>
    Task<TaskResult> ExecuteAsync(
        AgentContext context,
        IToolExecutor toolExecutor,
        CancellationToken cancellationToken = default);
}
