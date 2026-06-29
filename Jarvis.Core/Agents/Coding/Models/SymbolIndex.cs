namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents an index of source code symbols.
/// </summary>
public sealed class SymbolIndex
{
    /// <summary>
    /// Gets or sets indexed symbols.
    /// </summary>
    public List<CodeSymbol> Symbols { get; set; } = [];
}
