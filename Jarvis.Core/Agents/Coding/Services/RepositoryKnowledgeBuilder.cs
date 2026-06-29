namespace Jarvis.Core.Agents.Coding.Services;

using Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Builds factual repository knowledge from symbol and repository indexes.
/// </summary>
public sealed class RepositoryKnowledgeBuilder
{
    /// <summary>
    /// Builds repository knowledge.
    /// </summary>
    /// <param name="repositoryIndex">The repository index.</param>
    /// <param name="symbolIndex">The symbol index.</param>
    /// <returns>The repository knowledge.</returns>
    public RepositoryKnowledge Build(RepositoryIndex repositoryIndex, SymbolIndex symbolIndex)
    {
        ArgumentNullException.ThrowIfNull(repositoryIndex);
        ArgumentNullException.ThrowIfNull(symbolIndex);

        var classes = symbolIndex.Symbols.Where(symbol => symbol.Kind == "Class").ToList();
        var methods = symbolIndex.Symbols.Where(symbol => symbol.Kind == "Method").ToList();
        return new RepositoryKnowledge
        {
            SymbolCount = symbolIndex.Symbols.Count,
            ClassCount = classes.Count,
            MethodCount = methods.Count,
            InterfaceCount = CountKind(symbolIndex, "Interface"),
            PropertyCount = CountKind(symbolIndex, "Property"),
            FieldCount = CountKind(symbolIndex, "Field"),
            Namespaces = symbolIndex.Symbols
                .Where(symbol => symbol.Kind == "Namespace")
                .Select(symbol => symbol.Name)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList(),
            LargestClasses = classes
                .OrderByDescending(symbol => symbol.Children.Count)
                .ThenBy(symbol => symbol.Name, StringComparer.OrdinalIgnoreCase)
                .Take(10)
                .Select(symbol => $"{symbol.Name} ({symbol.Children.Count} members) - {symbol.File}:{symbol.Line}")
                .ToList(),
            LargestFiles = symbolIndex.Symbols
                .GroupBy(symbol => symbol.File, StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(group => group.Count())
                .ThenBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
                .Take(10)
                .Select(group => $"{group.Key} ({group.Count()} symbols)")
                .ToList(),
            AverageMethodsPerClass = classes.Count == 0
                ? 0
                : Math.Round((double)methods.Count / classes.Count, 2)
        };
    }

    private static int CountKind(SymbolIndex symbolIndex, string kind)
    {
        return symbolIndex.Symbols.Count(symbol => symbol.Kind.Equals(kind, StringComparison.OrdinalIgnoreCase));
    }
}
