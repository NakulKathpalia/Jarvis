namespace Jarvis.Core.Agents.Coding.Execution;

/// <summary>
/// Represents one live coding execution session.
/// </summary>
public sealed class ExecutionSession
{
    /// <summary>
    /// Gets or sets session identifier.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets or sets current state.
    /// </summary>
    public ExecutionState State { get; set; } = ExecutionState.Idle;

    /// <summary>
    /// Gets or sets start time.
    /// </summary>
    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets finish time.
    /// </summary>
    public DateTimeOffset? FinishedAt { get; set; }

    /// <summary>
    /// Gets execution events.
    /// </summary>
    public List<ExecutionEvent> Events { get; } = [];

    /// <summary>
    /// Gets progress.
    /// </summary>
    public ExecutionProgress Progress { get; } = new();
}
