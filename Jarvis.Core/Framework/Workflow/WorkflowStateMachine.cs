namespace Jarvis.Core.Framework.Workflow;

/// <summary>
/// Tracks workflow state and records accepted transitions.
/// </summary>
public sealed class WorkflowStateMachine
{
    private readonly WorkflowStateValidator validator;
    private readonly List<WorkflowTransition> transitions = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowStateMachine"/> class.
    /// </summary>
    /// <param name="initialState">The initial workflow state.</param>
    /// <param name="validator">The state transition validator.</param>
    public WorkflowStateMachine(WorkflowState initialState, WorkflowStateValidator validator)
    {
        this.CurrentState = initialState;
        this.validator = validator;
    }

    /// <summary>
    /// Gets the current workflow state.
    /// </summary>
    public WorkflowState CurrentState { get; private set; }

    /// <summary>
    /// Gets the accepted state transitions.
    /// </summary>
    public IReadOnlyList<WorkflowTransition> Transitions => transitions;

    /// <summary>
    /// Moves the workflow to a new state.
    /// </summary>
    /// <param name="state">The requested state.</param>
    /// <param name="reason">The transition reason.</param>
    public void TransitionTo(WorkflowState state, string reason)
    {
        validator.EnsureCanTransition(CurrentState, state);

        transitions.Add(new WorkflowTransition
        {
            From = CurrentState,
            To = state,
            Reason = reason
        });

        CurrentState = state;
    }
}
