namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents a compiler message.
/// </summary>
public class CompilerMessage
{
    /// <summary>
    /// Gets or sets the source file.
    /// </summary>
    public string File { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source line.
    /// </summary>
    public int Line { get; set; }

    /// <summary>
    /// Gets or sets the message code.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the message text.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
