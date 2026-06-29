namespace Jarvis.Core.Framework.Workflow;

/// <summary>
/// Defines a contract for a future remotely addressable agent.
/// </summary>
public interface IRemoteAgent
{
    /// <summary>
    /// Gets the remote agent name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Executes a remote request.
    /// </summary>
    /// <param name="request">The remote execution request.</param>
    /// <param name="cancellationToken">A token that cancels execution.</param>
    /// <returns>The remote execution result.</returns>
    Task<RemoteExecutionResult> ExecuteAsync(
        RemoteExecutionRequest request,
        CancellationToken cancellationToken = default);
}
