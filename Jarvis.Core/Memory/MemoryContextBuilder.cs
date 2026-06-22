using System.Text;
using System.Text.RegularExpressions;

namespace Jarvis.Memory;

public sealed class MemoryContextBuilder
{
    private static readonly Regex SensitiveValueRegex = new(
        @"(?i)\b(password|passcode|secret|token|api[_\s-]?key)\b\s*[:=]\s*\S+",
        RegexOptions.Compiled);

    public string Build(IReadOnlyList<MemoryRetrievalResult> memories, int maxMemories)
    {
        var selected = memories.Take(Math.Clamp(maxMemories, 1, 10)).ToList();
        if (selected.Count == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        builder.AppendLine("Relevant memories:");

        foreach (var result in selected)
        {
            var memory = result.Memory;
            var text = MaskSensitiveContent(memory.Text);
            builder.Append("* ");
            builder.Append(text);
            builder.Append(" [Category: ");
            builder.Append(memory.Category);
            builder.Append(", Importance: ");
            builder.Append(memory.Importance);
            builder.Append(", Confidence: ");
            builder.Append(memory.Confidence);
            builder.AppendLine("]");
        }

        return builder.ToString().TrimEnd();
    }

    private static string MaskSensitiveContent(string value)
    {
        return SensitiveValueRegex.Replace(value, match =>
        {
            var separatorIndex = Math.Max(match.Value.LastIndexOf(':'), match.Value.LastIndexOf('='));
            return separatorIndex < 0
                ? "[sensitive memory masked]"
                : $"{match.Value[..(separatorIndex + 1)]} [masked]";
        });
    }
}
