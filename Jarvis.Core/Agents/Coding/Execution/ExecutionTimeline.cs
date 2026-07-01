namespace Jarvis.Core.Agents.Coding.Execution;

/// <summary>
/// Stores completed and live execution events.
/// </summary>
public sealed class ExecutionTimeline
{
    private readonly object sync = new();

    /// <summary>
    /// Gets the execution session.
    /// </summary>
    public ExecutionSession Session { get; } = new();

    /// <summary>
    /// Adds an event to the timeline.
    /// </summary>
    public void Add(ExecutionEvent executionEvent)
    {
        lock (sync)
        {
            Session.Events.Add(executionEvent);
            Session.Progress.CurrentStage = executionEvent.Stage;
            Session.Progress.Elapsed = executionEvent.Elapsed;
            if (executionEvent.Type == ExecutionEventType.Error)
            {
                Session.State = ExecutionState.Failed;
            }
        }
    }

    /// <summary>
    /// Gets completed events.
    /// </summary>
    public IReadOnlyList<ExecutionEvent> CompletedEvents()
    {
        lock (sync)
        {
            return Session.Events.ToList();
        }
    }

    /// <summary>
    /// Gets warnings.
    /// </summary>
    public IReadOnlyList<ExecutionEvent> Warnings()
    {
        return CompletedEvents().Where(item => item.Type == ExecutionEventType.Warning).ToList();
    }

    /// <summary>
    /// Gets errors.
    /// </summary>
    public IReadOnlyList<ExecutionEvent> Errors()
    {
        return CompletedEvents().Where(item => item.Type == ExecutionEventType.Error).ToList();
    }
}
