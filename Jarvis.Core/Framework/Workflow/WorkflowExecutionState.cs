namespace Jarvis.Core.Framework.Workflow;

using Jarvis.Core.Framework.Models;

/// <summary>
/// Tracks in-memory state while a workflow is executing.
/// </summary>
public sealed class WorkflowExecutionState
{
    /// <summary>
    /// Gets completed step identifiers.
    /// </summary>
    public HashSet<string> CompletedStepIds { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets skipped step identifiers.
    /// </summary>
    public HashSet<string> SkippedStepIds { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets evaluated branch identifiers.
    /// </summary>
    public HashSet<string> EvaluatedBranchIds { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets completed step results by step identifier.
    /// </summary>
    public Dictionary<string, TaskResult> StepResults { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets currently running step identifiers.
    /// </summary>
    public HashSet<string> RunningStepIds { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets cancellation sources for running steps.
    /// </summary>
    public Dictionary<string, CancellationTokenSource> StepCancellationSources { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets the failed step identifier.
    /// </summary>
    public string FailedStepId { get; set; } = string.Empty;

    /// <summary>
    /// Gets a progress snapshot.
    /// </summary>
    /// <param name="totalSteps">The total number of steps.</param>
    /// <returns>The progress snapshot.</returns>
    public WorkflowProgress GetProgress(int totalSteps)
    {
        var completed = CompletedStepIds.Count + SkippedStepIds.Count;
        return new WorkflowProgress
        {
            TotalSteps = totalSteps,
            CompletedSteps = CompletedStepIds.Count,
            RunningStep = RunningStepIds.OrderBy(id => id, StringComparer.OrdinalIgnoreCase).FirstOrDefault() ?? string.Empty,
            RunningSteps = RunningStepIds.OrderBy(id => id, StringComparer.OrdinalIgnoreCase).ToList(),
            FailedStep = FailedStepId,
            ProgressPercent = totalSteps == 0 ? 0 : Math.Round((double)completed / totalSteps * 100, 2),
            EstimatedRemainingSteps = Math.Max(0, totalSteps - completed)
        };
    }

    /// <summary>
    /// Cancels a running step.
    /// </summary>
    /// <param name="stepId">The step identifier.</param>
    /// <returns><c>true</c> when a running step was cancelled.</returns>
    public bool CancelStep(string stepId)
    {
        if (StepCancellationSources.TryGetValue(stepId, out var source))
        {
            source.Cancel();
            return true;
        }

        return false;
    }
}
