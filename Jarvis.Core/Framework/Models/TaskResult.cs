namespace Jarvis.Core.Framework.Models;

/// <summary>
/// Represents the final result of a framework task execution.
/// </summary>
public sealed class TaskResult
{
    /// <summary>
    /// Gets or sets the associated request identifier.
    /// </summary>
    public string RequestId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether execution succeeded.
    /// </summary>
    public bool Succeeded { get; set; }

    /// <summary>
    /// Gets or sets the selected agent name.
    /// </summary>
    public string AgentName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the generic execution output.
    /// </summary>
    public string Output { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the failure message when execution fails.
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets tool results produced during execution.
    /// </summary>
    public List<ToolResult> ToolResults { get; set; } = [];
}
