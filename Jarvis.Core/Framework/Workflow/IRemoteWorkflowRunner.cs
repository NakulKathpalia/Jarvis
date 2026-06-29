namespace Jarvis.Core.Framework.Workflow;

/// <summary>
/// Defines a contract for future remote workflow execution.
/// </summary>
public interface IRemoteWorkflowRunner
{
    /// <summary>
    /// Executes a workflow remotely.
    /// </summary>
    /// <param name="request">The remote execution request.</param>
    /// <param name="cancellationToken">A token that cancels execution.</param>
    /// <returns>The remote execution result.</returns>
    Task<RemoteExecutionResult> ExecuteAsync(
        RemoteExecutionRequest request,
        CancellationToken cancellationToken = default);
}
