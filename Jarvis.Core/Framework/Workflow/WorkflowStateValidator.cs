namespace Jarvis.Core.Framework.Workflow;

/// <summary>
/// Validates legal workflow state transitions.
/// </summary>
public sealed class WorkflowStateValidator
{
    private static readonly IReadOnlyDictionary<WorkflowState, WorkflowState[]> Transitions =
        new Dictionary<WorkflowState, WorkflowState[]>
        {
            [WorkflowState.Created] = [WorkflowState.Queued, WorkflowState.Cancelled, WorkflowState.Failed],
            [WorkflowState.Queued] = [WorkflowState.Ready, WorkflowState.Cancelled, WorkflowState.Failed],
            [WorkflowState.Ready] = [WorkflowState.Running, WorkflowState.Cancelled, WorkflowState.Failed],
            [WorkflowState.Running] = [WorkflowState.Waiting, WorkflowState.Paused, WorkflowState.Retrying, WorkflowState.Completed, WorkflowState.Failed, WorkflowState.Cancelled],
            [WorkflowState.Waiting] = [WorkflowState.Running, WorkflowState.Cancelled, WorkflowState.Failed],
            [WorkflowState.Paused] = [WorkflowState.Running, WorkflowState.Cancelled, WorkflowState.Failed],
            [WorkflowState.Retrying] = [WorkflowState.Running, WorkflowState.Cancelled, WorkflowState.Failed],
            [WorkflowState.Completed] = [],
            [WorkflowState.Failed] = [],
            [WorkflowState.Cancelled] = []
        };

    /// <summary>
    /// Determines whether a state transition is legal.
    /// </summary>
    /// <param name="from">The current state.</param>
    /// <param name="to">The requested next state.</param>
    /// <returns><c>true</c> when the transition is allowed.</returns>
    public bool CanTransition(WorkflowState from, WorkflowState to)
    {
        return Transitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
    }

    /// <summary>
    /// Throws when a state transition is not legal.
    /// </summary>
    /// <param name="from">The current state.</param>
    /// <param name="to">The requested next state.</param>
    public void EnsureCanTransition(WorkflowState from, WorkflowState to)
    {
        if (!CanTransition(from, to))
        {
            throw new InvalidOperationException($"Workflow state transition from {from} to {to} is not allowed.");
        }
    }
}
