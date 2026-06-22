using System.Text.RegularExpressions;

namespace Jarvis.Services;

public static partial class JarvisInputNormalizer
{
    public static string StripAssistantPrefix(string input)
    {
        var trimmed = input.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return string.Empty;
        }

        var match = AssistantPrefixRegex().Match(trimmed);
        return match.Success
            ? trimmed[match.Length..].Trim()
            : trimmed;
    }

    [GeneratedRegex(@"^(?:hey\s+jarvis|okay\s+jarvis|ok\s+jarvis|jarvis)\b[\s,;:\-]*", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex AssistantPrefixRegex();
}
