namespace Jarvis.Core.Framework.Agents;

using Jarvis.Core.Framework.Models;

/// <summary>
/// Defines the manager responsible for executing task requests through agents.
/// </summary>
public interface IAgentManager
{
    /// <summary>
    /// Executes a task request through the planned agent.
    /// </summary>
    /// <param name="request">The task request.</param>
    /// <param name="cancellationToken">A token that cancels execution.</param>
    /// <returns>The task result.</returns>
    Task<TaskResult> ExecuteAsync(TaskRequest request, CancellationToken cancellationToken = default);
}
