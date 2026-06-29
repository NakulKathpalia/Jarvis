namespace Jarvis.Core.Framework.Workflow;

/// <summary>
/// Coordinates cooperative pause, resume, cancellation, and progress snapshots for a workflow run.
/// </summary>
public sealed class WorkflowControl
{
    private readonly object gate = new();
    private CancellationTokenSource? workflowCancellationSource;
    private WorkflowExecutionState? executionState;
    private WorkflowProgress progress = new();
    private string workflowId = string.Empty;
    private WorkflowState workflowState = WorkflowState.Created;
    private bool pauseRequested;
    private TaskCompletionSource<bool> resumeSignal = CompletedSignal();

    /// <summary>
    /// Attaches the active workflow cancellation source and execution state.
    /// </summary>
    /// <param name="cancellationSource">The workflow cancellation source.</param>
    /// <param name="state">The active execution state.</param>
    /// <param name="activeWorkflowId">The active workflow identifier.</param>
    /// <param name="activeWorkflowState">The active workflow state.</param>
    public void Attach(
        CancellationTokenSource cancellationSource,
        WorkflowExecutionState state,
        string activeWorkflowId,
        WorkflowState activeWorkflowState)
    {
        lock (gate)
        {
            workflowCancellationSource = cancellationSource;
            executionState = state;
            workflowId = activeWorkflowId;
            workflowState = activeWorkflowState;
        }
    }

    /// <summary>
    /// Clears the active workflow run.
    /// </summary>
    public void Detach()
    {
        lock (gate)
        {
            workflowCancellationSource = null;
            executionState = null;
            workflowId = string.Empty;
            workflowState = WorkflowState.Created;
        }
    }

    /// <summary>
    /// Updates the current workflow state used by checkpoint snapshots.
    /// </summary>
    /// <param name="state">The current workflow state.</param>
    public void UpdateState(WorkflowState state)
    {
        lock (gate)
        {
            workflowState = state;
        }
    }

    /// <summary>
    /// Requests pause before the next step batch starts.
    /// </summary>
    public void Pause()
    {
        lock (gate)
        {
            if (pauseRequested)
            {
                return;
            }

            pauseRequested = true;
            resumeSignal = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }

    /// <summary>
    /// Resumes a paused workflow.
    /// </summary>
    public void Resume()
    {
        lock (gate)
        {
            pauseRequested = false;
            resumeSignal.TrySetResult(true);
        }
    }

    /// <summary>
    /// Requests workflow cancellation.
    /// </summary>
    public void Cancel()
    {
        workflowCancellationSource?.Cancel();
    }

    /// <summary>
    /// Requests cancellation for one running step.
    /// </summary>
    /// <param name="stepId">The running step identifier.</param>
    /// <returns><c>true</c> when cancellation was requested.</returns>
    public bool CancelStep(string stepId)
    {
        return executionState?.CancelStep(stepId) ?? false;
    }

    /// <summary>
    /// Gets the current progress snapshot.
    /// </summary>
    /// <returns>The progress snapshot.</returns>
    public WorkflowProgress GetProgress()
    {
        lock (gate)
        {
            return new WorkflowProgress
            {
                TotalSteps = progress.TotalSteps,
                CompletedSteps = progress.CompletedSteps,
                RunningStep = progress.RunningStep,
                RunningSteps = [.. progress.RunningSteps],
                FailedStep = progress.FailedStep,
                ProgressPercent = progress.ProgressPercent,
                EstimatedRemainingSteps = progress.EstimatedRemainingSteps
            };
        }
    }

    /// <summary>
    /// Creates a workflow checkpoint snapshot from current runtime state.
    /// </summary>
    /// <returns>The workflow snapshot.</returns>
    public WorkflowSnapshot CreateCheckpoint()
    {
        lock (gate)
        {
            return new WorkflowSnapshot
            {
                WorkflowId = workflowId,
                State = workflowState,
                Progress = GetProgress(),
                CompletedStepIds = executionState?.CompletedStepIds.OrderBy(id => id, StringComparer.OrdinalIgnoreCase).ToList() ?? [],
                SkippedStepIds = executionState?.SkippedStepIds.OrderBy(id => id, StringComparer.OrdinalIgnoreCase).ToList() ?? []
            };
        }
    }

    /// <summary>
    /// Resets progress for a new workflow.
    /// </summary>
    /// <param name="totalSteps">The total step count.</param>
    public void ResetProgress(int totalSteps)
    {
        lock (gate)
        {
            progress = new WorkflowProgress
            {
                TotalSteps = totalSteps,
                EstimatedRemainingSteps = totalSteps
            };
        }
    }

    /// <summary>
    /// Updates progress from execution state.
    /// </summary>
    /// <param name="totalSteps">The total step count.</param>
    /// <param name="state">The execution state.</param>
    public void UpdateProgress(int totalSteps, WorkflowExecutionState state)
    {
        lock (gate)
        {
            progress = state.GetProgress(totalSteps);
        }
    }

    /// <summary>
    /// Gets the current pause wait task when pause is requested.
    /// </summary>
    /// <returns>The pause wait task, or <c>null</c> when not paused.</returns>
    public Task? GetPauseTask()
    {
        lock (gate)
        {
            return pauseRequested ? resumeSignal.Task : null;
        }
    }

    private static TaskCompletionSource<bool> CompletedSignal()
    {
        var source = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        source.SetResult(true);
        return source;
    }
}
