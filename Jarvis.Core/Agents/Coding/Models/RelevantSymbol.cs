namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents a symbol selected as factual coding context.
/// </summary>
public sealed class RelevantSymbol
{
    /// <summary>
    /// Gets or sets the symbol name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the symbol kind.
    /// </summary>
    public string Kind { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source file path.
    /// </summary>
    public string File { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the one-based source line.
    /// </summary>
    public int Line { get; set; }

    /// <summary>
    /// Gets or sets the parent symbol identifier.
    /// </summary>
    public string Parent { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the relevance score.
    /// </summary>
    public int Score { get; set; }
}
