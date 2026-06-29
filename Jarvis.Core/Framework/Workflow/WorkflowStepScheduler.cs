namespace Jarvis.Core.Framework.Workflow;

/// <summary>
/// Selects workflow steps that are ready for concurrent execution.
/// </summary>
public sealed class WorkflowStepScheduler
{
    /// <summary>
    /// Gets steps whose dependencies have completed or been skipped.
    /// </summary>
    /// <param name="steps">All workflow steps.</param>
    /// <param name="executionState">The current execution state.</param>
    /// <returns>The ready steps in stable workflow order.</returns>
    public IReadOnlyList<WorkflowStep> GetReadySteps(
        IEnumerable<WorkflowStep> steps,
        WorkflowExecutionState executionState)
    {
        ArgumentNullException.ThrowIfNull(steps);
        ArgumentNullException.ThrowIfNull(executionState);

        return steps
            .Where(step => !executionState.CompletedStepIds.Contains(step.Id))
            .Where(step => !executionState.SkippedStepIds.Contains(step.Id))
            .Where(step => step.Dependencies.All(dependency =>
                executionState.CompletedStepIds.Contains(dependency) ||
                executionState.SkippedStepIds.Contains(dependency)))
            .OrderBy(step => step.Order)
            .ThenBy(step => step.Id, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
