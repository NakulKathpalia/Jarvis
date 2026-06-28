namespace Jarvis.Core.Core.Logging;

/// <summary>
/// Represents a lightweight framework log entry.
/// </summary>
public sealed class FrameworkLogEntry
{
    /// <summary>
    /// Gets or sets the log level.
    /// </summary>
    public FrameworkLogLevel Level { get; set; } = FrameworkLogLevel.Information;

    /// <summary>
    /// Gets or sets the log message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the correlation identifier.
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the exception associated with the log entry.
    /// </summary>
    public Exception? Exception { get; set; }

    /// <summary>
    /// Gets or sets the log timestamp in UTC.
    /// </summary>
    public DateTimeOffset TimestampUtc { get; set; } = DateTimeOffset.UtcNow;
}
