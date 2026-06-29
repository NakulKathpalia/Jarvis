namespace Jarvis.Core.Agents.Coding.AI;

using System.Text.RegularExpressions;

/// <summary>
/// Parses plain model output into a coding suggestion.
/// </summary>
public sealed class CodingSuggestionParser
{
    private static readonly Regex FilePattern = new(@"(?im)^\s*(?:[-*]\s*)?(?:file|affected file)s?:?\s*(?<file>[\w./\\-]+\.\w+)\s*$");

    /// <summary>
    /// Parses model text.
    /// </summary>
    /// <param name="text">The model output.</param>
    /// <returns>The parsed suggestion.</returns>
    public CodingSuggestion Parse(string text)
    {
        text ??= string.Empty;
        return new CodingSuggestion
        {
            Explanation = ExtractExplanation(text),
            FilesAffected = ExtractFiles(text),
            SuggestedChanges = ExtractBullets(text, ["change", "step", "suggest"]),
            PatchText = ExtractPatch(text),
            SafetyWarnings = ExtractBullets(text, ["warning", "risk", "safety"])
        };
    }

    private static string ExtractExplanation(string text)
    {
        var patchIndex = text.IndexOf("```", StringComparison.Ordinal);
        var firstPart = patchIndex < 0 ? text : text[..patchIndex];
        return firstPart.Trim();
    }

    private static List<string> ExtractFiles(string text)
    {
        var files = FilePattern.Matches(text)
            .Select(match => match.Groups["file"].Value.Trim())
            .Where(file => !string.IsNullOrWhiteSpace(file))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (files.Count > 0)
        {
            return files;
        }

        return Regex.Matches(text, @"[\w./\\-]+\.(cs|ts|tsx|js|json|csproj)", RegexOptions.IgnoreCase)
            .Select(match => match.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string ExtractPatch(string text)
    {
        var match = Regex.Match(text, "```(?:diff|patch)?\\s*(?<patch>[\\s\\S]*?)```", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups["patch"].Value.Trim() : string.Empty;
    }

    private static List<string> ExtractBullets(string text, IReadOnlyCollection<string> keywords)
    {
        return text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => line.StartsWith('-') || line.StartsWith('*'))
            .Where(line => keywords.Count == 0 || keywords.Any(keyword => line.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            .Select(line => line.TrimStart('-', '*', ' '))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
