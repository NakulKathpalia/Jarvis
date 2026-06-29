namespace Jarvis.Core.Agents.Coding.Context;

using Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Builds factual coding context packages from repository and symbol indexes.
/// </summary>
public sealed class ContextBuilderV2
{
    private readonly ContextRanker ranker;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContextBuilderV2"/> class.
    /// </summary>
    /// <param name="ranker">The context ranker.</param>
    public ContextBuilderV2(ContextRanker? ranker = null)
    {
        this.ranker = ranker ?? new ContextRanker();
    }

    /// <summary>
    /// Builds a factual context package.
    /// </summary>
    public ContextPackage Build(
        ContextRequest request,
        RepositoryIndex repositoryIndex,
        SymbolIndex symbolIndex,
        RepositoryKnowledge knowledge)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(repositoryIndex);
        ArgumentNullException.ThrowIfNull(symbolIndex);
        ArgumentNullException.ThrowIfNull(knowledge);

        var terms = ranker.Tokenize(request.RequestText);
        var relevantSymbols = SelectSymbols(symbolIndex, terms, request.Window.MaxSymbols);
        var relevantFiles = SelectFiles(repositoryIndex, relevantSymbols, terms, request.Window);
        var relevantProjects = SelectProjects(repositoryIndex, relevantFiles, terms);
        var dependencies = SelectDependencies(repositoryIndex, relevantProjects);

        return new ContextPackage
        {
            Request = request,
            RelevantProjects = relevantProjects,
            RelevantFiles = relevantFiles,
            RelevantSymbols = relevantSymbols,
            RelatedInterfaces = relevantSymbols.Where(symbol => symbol.Kind == "Interface").ToList(),
            RelatedNamespaces = SelectNamespaces(knowledge, relevantSymbols),
            RelatedDependencies = dependencies,
            Statistics = new ContextStatistics
            {
                RelevantProjectCount = relevantProjects.Count,
                RelevantFileCount = relevantFiles.Count,
                RelevantSymbolCount = relevantSymbols.Count,
                DependencyCount = dependencies.Count
            }
        };
    }

    private List<RelevantSymbol> SelectSymbols(SymbolIndex symbolIndex, IReadOnlyList<string> terms, int maxSymbols)
    {
        return symbolIndex.Symbols
            .Select(symbol => new { Symbol = symbol, Score = ranker.ScoreSymbol(symbol, terms) })
            .Where(item => item.Score > 0)
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Symbol.File, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.Symbol.Line)
            .Take(Math.Max(1, maxSymbols))
            .Select(item => new RelevantSymbol
            {
                Name = item.Symbol.Name,
                Kind = item.Symbol.Kind,
                File = item.Symbol.File,
                Line = item.Symbol.Line,
                Parent = item.Symbol.Parent,
                Score = item.Score
            })
            .ToList();
    }

    private List<RelevantFile> SelectFiles(
        RepositoryIndex repositoryIndex,
        IReadOnlyList<RelevantSymbol> relevantSymbols,
        IReadOnlyList<string> terms,
        ContextWindow window)
    {
        var symbolFiles = relevantSymbols.Select(symbol => symbol.File).ToHashSet(StringComparer.OrdinalIgnoreCase);
        return repositoryIndex.Files.Files
            .Select(file => new
            {
                File = file,
                Score = ranker.ScoreFile(file, terms) + (symbolFiles.Contains(file.RelativePath) ? 5 : 0)
            })
            .Where(item => item.Score > 0)
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.File.RelativePath, StringComparer.OrdinalIgnoreCase)
            .Take(Math.Max(1, window.MaxFiles))
            .Select(item => CreateRelevantFile(item.File, item.Score, relevantSymbols, window.SnippetRadius))
            .ToList();
    }

    private static RelevantFile CreateRelevantFile(
        RepositoryFile file,
        int score,
        IReadOnlyList<RelevantSymbol> symbols,
        int snippetRadius)
    {
        var symbolLine = symbols.FirstOrDefault(symbol => symbol.File.Equals(file.RelativePath, StringComparison.OrdinalIgnoreCase))?.Line ?? 1;
        var source = ReadLines(file.Path);
        var startLine = Math.Max(1, symbolLine - snippetRadius);
        var endLine = Math.Min(source.Count, symbolLine + snippetRadius);
        return new RelevantFile
        {
            Path = file.RelativePath,
            Language = file.Language,
            Score = score,
            StartLine = source.Count == 0 ? 0 : startLine,
            EndLine = source.Count == 0 ? 0 : endLine,
            SourceSnippet = source.Count == 0 ? string.Empty : string.Join(Environment.NewLine, source.Skip(startLine - 1).Take(endLine - startLine + 1)),
            ImportStatements = source.Where(IsImportStatement).Take(30).ToList()
        };
    }

    private List<RelevantProject> SelectProjects(
        RepositoryIndex repositoryIndex,
        IReadOnlyList<RelevantFile> relevantFiles,
        IReadOnlyList<string> terms)
    {
        var relevantPaths = relevantFiles.Select(file => file.Path).ToList();
        return repositoryIndex.Projects.Projects
            .Select(project => new
            {
                Project = project,
                Score = ranker.ScoreProject(project, terms) + relevantPaths.Count(path => path.StartsWith(ProjectFolder(project), StringComparison.OrdinalIgnoreCase))
            })
            .Where(item => item.Score > 0)
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Project.RelativePath, StringComparer.OrdinalIgnoreCase)
            .Select(item => new RelevantProject
            {
                Name = item.Project.Name,
                Path = item.Project.RelativePath,
                ProjectType = item.Project.ProjectType,
                ProjectReferences = item.Project.ProjectReferences.Select(reference => reference.ReferencePath).ToList()
            })
            .ToList();
    }

    private static List<string> SelectDependencies(RepositoryIndex repositoryIndex, IReadOnlyList<RelevantProject> projects)
    {
        var names = projects.Select(project => project.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        return repositoryIndex.Dependencies
            .Where(dependency => names.Count == 0 || names.Contains(dependency.SourceProject))
            .Select(dependency => $"{dependency.SourceProject} -> {dependency.ReferencePath}")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(dependency => dependency, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static List<string> SelectNamespaces(RepositoryKnowledge knowledge, IReadOnlyList<RelevantSymbol> symbols)
    {
        var files = symbols.Select(symbol => symbol.File).ToHashSet(StringComparer.OrdinalIgnoreCase);
        return knowledge.Namespaces
            .Where(ns => symbols.Any(symbol => symbol.Name.Contains(ns, StringComparison.OrdinalIgnoreCase)) || files.Count > 0)
            .Take(30)
            .ToList();
    }

    private static string ProjectFolder(RepositoryProject project)
    {
        var slash = project.RelativePath.LastIndexOf('/');
        return slash < 0 ? string.Empty : project.RelativePath[..slash];
    }

    private static List<string> ReadLines(string path)
    {
        try
        {
            return File.ReadAllLines(path).ToList();
        }
        catch
        {
            return [];
        }
    }

    private static bool IsImportStatement(string line)
    {
        var trimmed = line.TrimStart();
        return trimmed.StartsWith("using ", StringComparison.Ordinal) ||
            trimmed.StartsWith("import ", StringComparison.Ordinal);
    }
}
