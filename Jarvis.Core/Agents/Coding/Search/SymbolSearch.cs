namespace Jarvis.Core.Agents.Coding.Search;

using Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Provides exact lookup over a symbol index.
/// </summary>
public sealed class SymbolSearch
{
    /// <summary>
    /// Finds symbols by optional exact filters.
    /// </summary>
    public IReadOnlyList<CodeSymbol> Find(
        SymbolIndex index,
        string name = "",
        string kind = "",
        string language = "",
        string file = "",
        string project = "")
    {
        ArgumentNullException.ThrowIfNull(index);

        return index.Symbols
            .Where(symbol => Exact(symbol.Name, name))
            .Where(symbol => Exact(symbol.Kind, kind))
            .Where(symbol => Exact(symbol.Language, language))
            .Where(symbol => Exact(symbol.File, file))
            .Where(symbol => Exact(symbol.Project, project))
            .OrderBy(symbol => symbol.File, StringComparer.OrdinalIgnoreCase)
            .ThenBy(symbol => symbol.Line)
            .ToList();
    }

    /// <summary>
    /// Finds class symbols by exact name.
    /// </summary>
    public IReadOnlyList<CodeSymbol> FindClass(SymbolIndex index, string name) => Find(index, name, "Class");

    /// <summary>
    /// Finds method symbols by exact name.
    /// </summary>
    public IReadOnlyList<CodeSymbol> FindMethod(SymbolIndex index, string name) => Find(index, name, "Method");

    /// <summary>
    /// Finds interface symbols by exact name.
    /// </summary>
    public IReadOnlyList<CodeSymbol> FindInterface(SymbolIndex index, string name) => Find(index, name, "Interface");

    /// <summary>
    /// Finds enum symbols by exact name.
    /// </summary>
    public IReadOnlyList<CodeSymbol> FindEnum(SymbolIndex index, string name) => Find(index, name, "Enum");

    /// <summary>
    /// Finds namespace symbols by exact name.
    /// </summary>
    public IReadOnlyList<CodeSymbol> FindNamespace(SymbolIndex index, string name) => Find(index, name, "Namespace");

    /// <summary>
    /// Finds property symbols by exact name.
    /// </summary>
    public IReadOnlyList<CodeSymbol> FindProperty(SymbolIndex index, string name) => Find(index, name, "Property");

    /// <summary>
    /// Finds field symbols by exact name.
    /// </summary>
    public IReadOnlyList<CodeSymbol> FindField(SymbolIndex index, string name) => Find(index, name, "Field");

    /// <summary>
    /// Finds constructor symbols by exact name.
    /// </summary>
    public IReadOnlyList<CodeSymbol> FindConstructor(SymbolIndex index, string name) => Find(index, name, "Constructor");

    private static bool Exact(string value, string query)
    {
        return string.IsNullOrWhiteSpace(query) ||
            string.Equals(value, query, StringComparison.OrdinalIgnoreCase);
    }
}
