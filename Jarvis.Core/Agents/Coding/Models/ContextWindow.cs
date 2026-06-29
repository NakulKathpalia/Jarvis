namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents factual context window limits.
/// </summary>
public sealed class ContextWindow
{
    /// <summary>
    /// Gets or sets the maximum relevant file count.
    /// </summary>
    public int MaxFiles { get; set; } = 12;

    /// <summary>
    /// Gets or sets the maximum relevant symbol count.
    /// </summary>
    public int MaxSymbols { get; set; } = 30;

    /// <summary>
    /// Gets or sets the source snippet radius around a symbol line.
    /// </summary>
    public int SnippetRadius { get; set; } = 8;
}
