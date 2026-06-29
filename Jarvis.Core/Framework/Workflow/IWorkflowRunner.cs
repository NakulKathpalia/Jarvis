namespace Jarvis.Core.Framework.Workflow;

/// <summary>
/// Defines a workflow runner that executes workflow steps through the framework.
/// </summary>
public interface IWorkflowRunner
{
    /// <summary>
    /// Runs a workflow.
    /// </summary>
    /// <param name="workflow">The workflow to run.</param>
    /// <param name="cancellationToken">A token that cancels execution.</param>
    /// <returns>The workflow result.</returns>
    Task<WorkflowResult> RunAsync(Workflow workflow, CancellationToken cancellationToken = default);

    /// <summary>
    /// Requests workflow pause before the next step batch starts.
    /// </summary>
    void Pause();

    /// <summary>
    /// Resumes a paused workflow.
    /// </summary>
    void Resume();

    /// <summary>
    /// Requests cancellation of the running workflow.
    /// </summary>
    void Cancel();

    /// <summary>
    /// Requests cancellation of a running step.
    /// </summary>
    /// <param name="stepId">The running step identifier.</param>
    /// <returns><c>true</c> when the step was running and cancellation was requested.</returns>
    bool CancelStep(string stepId);

    /// <summary>
    /// Gets the current workflow progress snapshot.
    /// </summary>
    /// <returns>The current progress.</returns>
    WorkflowProgress GetProgress();

    /// <summary>
    /// Creates a checkpoint snapshot from the current workflow runtime state.
    /// </summary>
    /// <returns>The workflow snapshot.</returns>
    WorkflowSnapshot CreateCheckpoint();

    /// <summary>
    /// Gets the current workflow metrics snapshot.
    /// </summary>
    /// <returns>The workflow metrics.</returns>
    WorkflowMetrics GetMetrics();
}
