namespace Jarvis.Core.Framework.Events;

/// <summary>
/// Represents an in-process framework event.
/// </summary>
public sealed class FrameworkEvent
{
    /// <summary>
    /// Gets or sets the event identifier.
    /// </summary>
    public string EventId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets or sets the event name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the event correlation identifier.
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the event payload.
    /// </summary>
    public object? Payload { get; set; }

    /// <summary>
    /// Gets or sets the event creation time in UTC.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
