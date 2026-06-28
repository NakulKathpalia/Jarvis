namespace Jarvis.Core.Framework.Routing;

using Jarvis.Core.Framework.Models;

/// <summary>
/// Defines the main framework task execution pipeline.
/// </summary>
public interface ITaskPipeline
{
    /// <summary>
    /// Executes a task request through the framework runtime.
    /// </summary>
    /// <param name="request">The task request.</param>
    /// <param name="cancellationToken">A token that cancels execution.</param>
    /// <returns>The task result.</returns>
    Task<TaskResult> ExecuteAsync(TaskRequest request, CancellationToken cancellationToken = default);
}
