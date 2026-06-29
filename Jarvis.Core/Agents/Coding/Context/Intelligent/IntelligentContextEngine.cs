namespace Jarvis.Core.Agents.Coding.Context.Intelligent;

using Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Selects, ranks, and compresses AI-ready coding context.
/// </summary>
public sealed class IntelligentContextEngine
{
    private readonly ContextRelevanceScorer scorer;
    private readonly ContextWindowBuilder windowBuilder;

    /// <summary>
    /// Initializes a new instance of the <see cref="IntelligentContextEngine"/> class.
    /// </summary>
    public IntelligentContextEngine(ContextRelevanceScorer? scorer = null, ContextWindowBuilder? windowBuilder = null)
    {
        this.scorer = scorer ?? new ContextRelevanceScorer();
        this.windowBuilder = windowBuilder ?? new ContextWindowBuilder();
    }

    /// <summary>
    /// Builds intelligent context.
    /// </summary>
    public IntelligentContextResult Build(IntelligentContextRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var terms = scorer.Tokenize(request.UserRequest);
        var files = SelectFiles(request.RepositoryContext.ContextPackage.RelevantFiles, terms, request);
        var symbols = SelectSymbols(request.RepositoryContext.ContextPackage.RelevantSymbols, terms, request);
        var original = windowBuilder.Build(request.UserRequest, request.RepositoryContext, files, symbols, request.Budget);
        var compressed = Compress(original, request);
        var result = new IntelligentContextResult
        {
            Succeeded = true,
            SelectedFiles = files,
            SelectedSymbols = symbols,
            ContextText = compressed,
            OriginalSize = original.Length,
            CompressedSize = compressed.Length
        };

        if (files.Count == 0)
        {
            result.Warnings.Add("No files selected for intelligent context.");
        }

        if (symbols.Count == 0)
        {
            result.Warnings.Add("No symbols selected for intelligent context.");
        }

        return result;
    }

    private List<RelevantFile> SelectFiles(
        IReadOnlyList<RelevantFile> files,
        IReadOnlyCollection<string> terms,
        IntelligentContextRequest request)
    {
        var maxFiles = request.Strategy == ContextSelectionStrategy.Minimal
            ? Math.Min(4, request.Budget.MaxFiles)
            : request.Budget.MaxFiles;
        return files
            .GroupBy(file => file.Path, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderByDescending(file => scorer.ScoreFile(file, terms))
            .ThenBy(file => file.Path, StringComparer.OrdinalIgnoreCase)
            .Take(Math.Max(1, maxFiles))
            .ToList();
    }

    private List<RelevantSymbol> SelectSymbols(
        IReadOnlyList<RelevantSymbol> symbols,
        IReadOnlyCollection<string> terms,
        IntelligentContextRequest request)
    {
        var maxSymbols = request.Strategy == ContextSelectionStrategy.FileFocused
            ? Math.Min(12, request.Budget.MaxSymbols)
            : request.Budget.MaxSymbols;
        return symbols
            .GroupBy(symbol => $"{symbol.Kind}:{symbol.Name}:{symbol.File}:{symbol.Line}", StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderByDescending(symbol => scorer.ScoreSymbol(symbol, terms))
            .ThenBy(symbol => symbol.File, StringComparer.OrdinalIgnoreCase)
            .Take(Math.Max(1, maxSymbols))
            .ToList();
    }

    private static string Compress(string context, IntelligentContextRequest request)
    {
        if (request.CompressionPolicy == ContextCompressionPolicy.None ||
            context.Length <= request.Budget.MaxCharacters)
        {
            return context;
        }

        var lines = context.Split(Environment.NewLine);
        var maxSnippetLines = request.CompressionPolicy == ContextCompressionPolicy.Aggressive
            ? Math.Min(16, request.Budget.MaxSnippetLines)
            : request.Budget.MaxSnippetLines;
        var output = new List<string>();
        var imports = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var snippetLines = 0;
        var inSnippet = false;
        foreach (var line in lines)
        {
            if (line.Trim().Equals("snippet:", StringComparison.OrdinalIgnoreCase))
            {
                inSnippet = true;
                snippetLines = 0;
                output.Add(line);
                continue;
            }

            if (line.StartsWith("- ", StringComparison.Ordinal))
            {
                inSnippet = false;
            }

            if (line.TrimStart().StartsWith("import:", StringComparison.OrdinalIgnoreCase) &&
                !imports.Add(line.Trim()))
            {
                continue;
            }

            if (inSnippet && snippetLines++ >= maxSnippetLines)
            {
                continue;
            }

            output.Add(line);
            if (string.Join(Environment.NewLine, output).Length >= request.Budget.MaxCharacters)
            {
                break;
            }
        }

        return string.Join(Environment.NewLine, output);
    }
}
