namespace Jarvis.Core.Framework.Workflow;

/// <summary>
/// Represents the lifecycle state of a workflow.
/// </summary>
public enum WorkflowState
{
    /// <summary>
    /// The workflow has been created but not queued.
    /// </summary>
    Created,

    /// <summary>
    /// The workflow is queued for execution.
    /// </summary>
    Queued,

    /// <summary>
    /// The workflow has passed validation and is ready to run.
    /// </summary>
    Ready,

    /// <summary>
    /// The workflow is currently running.
    /// </summary>
    Running,

    /// <summary>
    /// The workflow is waiting for an external condition.
    /// </summary>
    Waiting,

    /// <summary>
    /// The workflow is paused.
    /// </summary>
    Paused,

    /// <summary>
    /// The workflow is preparing to retry.
    /// </summary>
    Retrying,

    /// <summary>
    /// The workflow completed successfully.
    /// </summary>
    Completed,

    /// <summary>
    /// The workflow failed.
    /// </summary>
    Failed,

    /// <summary>
    /// The workflow was cancelled.
    /// </summary>
    Cancelled
}
