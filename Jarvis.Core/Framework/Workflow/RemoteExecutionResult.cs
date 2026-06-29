namespace Jarvis.Core.Framework.Workflow;

/// <summary>
/// Represents a future remote workflow or agent execution result.
/// </summary>
public sealed class RemoteExecutionResult
{
    /// <summary>
    /// Gets or sets the request identifier.
    /// </summary>
    public string RequestId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether remote execution succeeded.
    /// </summary>
    public bool Succeeded { get; set; }

    /// <summary>
    /// Gets or sets the remote result output.
    /// </summary>
    public string Output { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the remote error message.
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
}
