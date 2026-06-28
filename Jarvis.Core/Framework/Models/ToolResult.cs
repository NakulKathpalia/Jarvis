namespace Jarvis.Core.Framework.Models;

/// <summary>
/// Represents the result of a single tool execution.
/// </summary>
public sealed class ToolResult
{
    /// <summary>
    /// Gets or sets the tool name.
    /// </summary>
    public string ToolName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the tool succeeded.
    /// </summary>
    public bool Succeeded { get; set; }

    /// <summary>
    /// Gets or sets the generic tool output.
    /// </summary>
    public string Output { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the failure message when execution fails.
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
}
