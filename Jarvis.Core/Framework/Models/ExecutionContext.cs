namespace Jarvis.Core.Framework.Models;

/// <summary>
/// Represents shared state for a single framework execution.
/// </summary>
public sealed class ExecutionContext
{
    /// <summary>
    /// Gets or sets the correlation identifier for the execution.
    /// </summary>
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets or sets the request being executed.
    /// </summary>
    public TaskRequest Request { get; set; } = new();

    /// <summary>
    /// Gets or sets the selected agent name.
    /// </summary>
    public string AgentName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets per-execution items shared across the pipeline.
    /// </summary>
    public Dictionary<string, object?> Items { get; set; } = [];

    /// <summary>
    /// Gets or sets the context creation time in UTC.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
