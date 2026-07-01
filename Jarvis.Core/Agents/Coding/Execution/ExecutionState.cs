namespace Jarvis.Core.Agents.Coding.Execution;

/// <summary>
/// Defines timeline execution state.
/// </summary>
public enum ExecutionState
{
    Idle,
    Running,
    Waiting,
    Completed,
    Failed,
    Cancelled
}
