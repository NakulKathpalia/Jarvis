namespace Jarvis.Core.Brain.Intent;

using Jarvis.Core.Brain.Interfaces;
using Jarvis.Core.Brain.Models;

/// <summary>
/// Provides rule-based intent analysis.
/// </summary>
public sealed class IntentAnalyzer : IIntentAnalyzer
{
    private static readonly IReadOnlyDictionary<string, string> KeywordTaskTypes =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["build"] = "Coding",
            ["search"] = "Research",
            ["open"] = "Browser",
            ["remember"] = "Memory",
            ["voice"] = "Voice"
        };

    /// <inheritdoc />
    public IntentResult Analyze(string input)
    {
        var normalizedInput = input ?? string.Empty;

        foreach (var pair in KeywordTaskTypes)
        {
            if (normalizedInput.Contains(pair.Key, StringComparison.OrdinalIgnoreCase))
            {
                return new IntentResult
                {
                    Intent = pair.Key,
                    TaskType = pair.Value,
                    Confidence = 0.75,
                    Reason = $"Matched keyword: {pair.Key}"
                };
            }
        }

        return new IntentResult
        {
            Intent = "General",
            TaskType = "General",
            Confidence = 0.25,
            Reason = "No rule matched."
        };
    }
}
