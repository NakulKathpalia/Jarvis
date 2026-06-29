namespace Jarvis.Core.Framework.Workflow;

/// <summary>
/// Represents a future remote workflow or agent execution request.
/// </summary>
public sealed class RemoteExecutionRequest
{
    /// <summary>
    /// Gets or sets the request identifier.
    /// </summary>
    public string RequestId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets or sets the target runner or agent name.
    /// </summary>
    public string Target { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the workflow to execute remotely.
    /// </summary>
    public Workflow? Workflow { get; set; }

    /// <summary>
    /// Gets or sets remote execution parameters.
    /// </summary>
    public Dictionary<string, object?> Parameters { get; set; } = [];
}
