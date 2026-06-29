namespace Jarvis.Core.Agents.Coding.Context.Intelligent;

/// <summary>
/// Defines how intelligent context should be selected.
/// </summary>
public enum ContextSelectionStrategy
{
    /// <summary>
    /// Balance files, symbols, and snippets.
    /// </summary>
    Balanced,

    /// <summary>
    /// Prefer source files and snippets.
    /// </summary>
    FileFocused,

    /// <summary>
    /// Prefer symbols and declarations.
    /// </summary>
    SymbolFocused,

    /// <summary>
    /// Prefer the smallest useful context.
    /// </summary>
    Minimal
}
