namespace Jarvis.Core.Agents.Coding.Execution;

/// <summary>
/// Represents one live execution event.
/// </summary>
public sealed class ExecutionEvent
{
    /// <summary>
    /// Gets or sets the event identifier.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets or sets the stage.
    /// </summary>
    public ExecutionStage Stage { get; set; }

    /// <summary>
    /// Gets or sets the event type.
    /// </summary>
    public ExecutionEventType Type { get; set; }

    /// <summary>
    /// Gets or sets event status.
    /// </summary>
    public ExecutionStatus Status { get; set; }

    /// <summary>
    /// Gets or sets event message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets current tool name.
    /// </summary>
    public string Tool { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets current agent name.
    /// </summary>
    public string Agent { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets timestamp.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets duration since session start.
    /// </summary>
    public TimeSpan Elapsed { get; set; }
}
