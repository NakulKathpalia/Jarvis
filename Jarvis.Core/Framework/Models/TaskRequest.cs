namespace Jarvis.Core.Framework.Models;

/// <summary>
/// Represents a generic request submitted to the Jarvis framework runtime.
/// </summary>
public sealed class TaskRequest
{
    /// <summary>
    /// Gets or sets the request identifier.
    /// </summary>
    public string RequestId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets or sets the generic task type used by the planner.
    /// </summary>
    public string TaskType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the request input.
    /// </summary>
    public string Input { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets optional request parameters.
    /// </summary>
    public Dictionary<string, object?> Parameters { get; set; } = [];

    /// <summary>
    /// Gets or sets the request creation time in UTC.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
