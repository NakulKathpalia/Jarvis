namespace Jarvis.Core.Agents.Coding.Execution;

/// <summary>
/// Defines status for one execution event.
/// </summary>
public enum ExecutionStatus
{
    Pending,
    Active,
    Succeeded,
    Failed,
    Skipped
}
