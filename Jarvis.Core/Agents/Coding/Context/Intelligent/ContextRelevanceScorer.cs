namespace Jarvis.Core.Agents.Coding.Context.Intelligent;

using Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Scores files and symbols for a coding request.
/// </summary>
public sealed class ContextRelevanceScorer
{
    /// <summary>
    /// Scores a relevant file.
    /// </summary>
    public int ScoreFile(RelevantFile file, IReadOnlyCollection<string> terms)
    {
        var score = file.Score;
        score += terms.Count(term => file.Path.Contains(term, StringComparison.OrdinalIgnoreCase)) * 5;
        score += terms.Count(term => file.SourceSnippet.Contains(term, StringComparison.OrdinalIgnoreCase)) * 2;
        return score;
    }

    /// <summary>
    /// Scores a relevant symbol.
    /// </summary>
    public int ScoreSymbol(RelevantSymbol symbol, IReadOnlyCollection<string> terms)
    {
        var score = symbol.Score;
        score += terms.Count(term => symbol.Name.Contains(term, StringComparison.OrdinalIgnoreCase)) * 6;
        score += terms.Count(term => symbol.File.Contains(term, StringComparison.OrdinalIgnoreCase)) * 2;
        return score;
    }

    /// <summary>
    /// Tokenizes request text for ranking.
    /// </summary>
    public IReadOnlyList<string> Tokenize(string text)
    {
        return (text ?? string.Empty)
            .Split([' ', '.', '-', '_', '/', '\\', ':', ';', ',', '(', ')'], StringSplitOptions.RemoveEmptyEntries)
            .Where(term => term.Length > 2)
            .Select(term => term.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
