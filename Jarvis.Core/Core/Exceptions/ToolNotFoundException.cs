namespace Jarvis.Core.Core.Exceptions;

/// <summary>
/// Represents a failure to resolve a requested tool.
/// </summary>
public sealed class ToolNotFoundException : FrameworkException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ToolNotFoundException"/> class.
    /// </summary>
    /// <param name="toolName">The unresolved tool name.</param>
    public ToolNotFoundException(string toolName)
        : base($"Tool was not found: {toolName}")
    {
        ToolName = toolName;
    }

    /// <summary>
    /// Gets the unresolved tool name.
    /// </summary>
    public string ToolName { get; }
}
