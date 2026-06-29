namespace Jarvis.Core.Framework.Workflow;

/// <summary>
/// Defines persistence contracts for workflow snapshots.
/// </summary>
public interface IWorkflowStore
{
    /// <summary>
    /// Saves a workflow snapshot.
    /// </summary>
    /// <param name="snapshot">The snapshot to save.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>A task that completes when the snapshot has been saved.</returns>
    Task SaveSnapshotAsync(WorkflowSnapshot snapshot, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads the latest workflow snapshot.
    /// </summary>
    /// <param name="workflowId">The workflow identifier.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>The latest snapshot, or <c>null</c> when none exists.</returns>
    Task<WorkflowSnapshot?> LoadLatestSnapshotAsync(string workflowId, CancellationToken cancellationToken = default);
}
