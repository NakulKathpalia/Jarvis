namespace Jarvis.Core.Agents.Coding.Context.Intelligent;

using Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents AI-ready coding context.
/// </summary>
public sealed class IntelligentContextResult
{
    /// <summary>
    /// Gets or sets a value indicating whether context generation succeeded.
    /// </summary>
    public bool Succeeded { get; set; }

    /// <summary>
    /// Gets or sets selected files.
    /// </summary>
    public List<RelevantFile> SelectedFiles { get; set; } = [];

    /// <summary>
    /// Gets or sets selected symbols.
    /// </summary>
    public List<RelevantSymbol> SelectedSymbols { get; set; } = [];

    /// <summary>
    /// Gets or sets the generated context text.
    /// </summary>
    public string ContextText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the size before compression.
    /// </summary>
    public int OriginalSize { get; set; }

    /// <summary>
    /// Gets or sets the size after compression.
    /// </summary>
    public int CompressedSize { get; set; }

    /// <summary>
    /// Gets warnings produced while building context.
    /// </summary>
    public List<string> Warnings { get; } = [];
}
