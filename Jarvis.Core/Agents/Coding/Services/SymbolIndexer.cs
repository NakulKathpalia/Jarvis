namespace Jarvis.Core.Agents.Coding.Services;

using Jarvis.Core.Agents.Coding.Models;
using Jarvis.Core.Agents.Coding.Parsing;

/// <summary>
/// Builds a symbol index from repository source files.
/// </summary>
public sealed class SymbolIndexer
{
    private readonly LanguageParserRegistry parserRegistry;

    /// <summary>
    /// Initializes a new instance of the <see cref="SymbolIndexer"/> class.
    /// </summary>
    /// <param name="parserRegistry">The language parser registry.</param>
    public SymbolIndexer(LanguageParserRegistry parserRegistry)
    {
        this.parserRegistry = parserRegistry;
    }

    /// <summary>
    /// Builds a symbol index.
    /// </summary>
    /// <param name="repositoryIndex">The repository index.</param>
    /// <returns>The symbol index.</returns>
    public SymbolIndex Build(RepositoryIndex repositoryIndex)
    {
        ArgumentNullException.ThrowIfNull(repositoryIndex);

        var symbols = new List<CodeSymbol>();
        foreach (var file in repositoryIndex.Files.Files.Where(file => !string.IsNullOrWhiteSpace(file.Language)))
        {
            if (!parserRegistry.TryGetParser(file.Language, out var parser) || parser is null)
            {
                continue;
            }

            symbols.AddRange(ParseFile(parser, file));
        }

        return new SymbolIndex
        {
            Symbols = symbols
                .OrderBy(symbol => symbol.File, StringComparer.OrdinalIgnoreCase)
                .ThenBy(symbol => symbol.Line)
                .ThenBy(symbol => symbol.Name, StringComparer.OrdinalIgnoreCase)
                .ToList()
        };
    }

    private static IReadOnlyList<CodeSymbol> ParseFile(ILanguageParser parser, RepositoryFile file)
    {
        try
        {
            return parser.Parse(file, File.ReadAllText(file.Path));
        }
        catch
        {
            return [];
        }
    }
}
