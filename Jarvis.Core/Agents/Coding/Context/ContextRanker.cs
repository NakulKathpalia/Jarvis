namespace Jarvis.Core.Agents.Coding.Context;

using Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Ranks repository artifacts using literal request terms.
/// </summary>
public sealed class ContextRanker
{
    /// <summary>
    /// Scores a repository file.
    /// </summary>
    public int ScoreFile(RepositoryFile file, IReadOnlyList<string> terms)
    {
        var score = ScoreText(file.RelativePath, terms) + ScoreText(file.Language, terms);
        return score == 0 && IsLikelyRequestArea(file.RelativePath, terms) ? 1 : score;
    }

    /// <summary>
    /// Scores a code symbol.
    /// </summary>
    public int ScoreSymbol(CodeSymbol symbol, IReadOnlyList<string> terms)
    {
        return ScoreText(symbol.Name, terms) * 3 +
            ScoreText(symbol.Kind, terms) +
            ScoreText(symbol.File, terms);
    }

    /// <summary>
    /// Scores a project.
    /// </summary>
    public int ScoreProject(RepositoryProject project, IReadOnlyList<string> terms)
    {
        return ScoreText(project.Name, terms) * 2 + ScoreText(project.RelativePath, terms);
    }

    /// <summary>
    /// Tokenizes request text into literal matching terms.
    /// </summary>
    public IReadOnlyList<string> Tokenize(string requestText)
    {
        return requestText
            .Split([' ', '-', '_', '.', '/', '\\', ':', ';', ',', '(', ')', '[', ']'], StringSplitOptions.RemoveEmptyEntries)
            .Select(term => term.Trim())
            .Where(term => term.Length > 2)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static int ScoreText(string text, IReadOnlyList<string> terms)
    {
        return terms.Count(term => text.Contains(term, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsLikelyRequestArea(string path, IReadOnlyList<string> terms)
    {
        return terms.Any(term =>
            path.Contains("auth", StringComparison.OrdinalIgnoreCase) && term.Contains("auth", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("security", StringComparison.OrdinalIgnoreCase) && term.Contains("jwt", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("settings", StringComparison.OrdinalIgnoreCase) && term.Contains("jwt", StringComparison.OrdinalIgnoreCase));
    }
}
