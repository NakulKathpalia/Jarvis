namespace Jarvis.Core.AI.Prompt;

using Jarvis.Core.AI.Runtime;

/// <summary>
/// Validates prompts before provider execution.
/// </summary>
public sealed class PromptValidator
{
    /// <summary>
    /// Validates a request.
    /// </summary>
    public IReadOnlyList<string> Validate(AIRequest request)
    {
        var warnings = new List<string>();
        if (string.IsNullOrWhiteSpace(request.Prompt))
        {
            warnings.Add("Prompt is empty.");
        }

        if (request.Prompt.Length > GetPromptLimit(request))
        {
            warnings.Add("Prompt exceeds configured context limit and will be compressed.");
        }

        if (request.Context.FilePaths.Count == 0)
        {
            warnings.Add("Repository context does not include file paths.");
        }

        if (request.Context.Symbols.Count == 0)
        {
            warnings.Add("Repository context does not include symbols.");
        }

        AddDuplicateWarnings("Duplicate file context detected", request.Context.FilePaths, warnings);
        AddDuplicateWarnings("Duplicate symbol context detected", request.Context.Symbols, warnings);

        if (request.Prompt.Contains("snippet:", StringComparison.OrdinalIgnoreCase) &&
            request.Prompt.Contains("snippet:\r\n\r\n", StringComparison.OrdinalIgnoreCase))
        {
            warnings.Add("Prompt contains an empty snippet block.");
        }

        return warnings;
    }

    private static int GetPromptLimit(AIRequest request)
    {
        return Math.Max(2048, (request.Options.NumContext ?? 8192) * 4);
    }

    private static void AddDuplicateWarnings(string message, IEnumerable<string> values, ICollection<string> warnings)
    {
        var duplicateFound = values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .GroupBy(value => value, StringComparer.OrdinalIgnoreCase)
            .Any(group => group.Count() > 1);
        if (duplicateFound)
        {
            warnings.Add(message);
        }
    }
}
