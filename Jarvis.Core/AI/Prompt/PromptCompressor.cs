namespace Jarvis.Core.AI.Prompt;

using Jarvis.Core.AI.Runtime;

/// <summary>
/// Compresses prompts that exceed the configured context budget.
/// </summary>
public sealed class PromptCompressor
{
    /// <summary>
    /// Compresses a prompt while preserving the user request.
    /// </summary>
    public string Compress(AIRequest request)
    {
        var prompt = request.Prompt ?? string.Empty;
        var limit = Math.Max(2048, (request.Options.NumContext ?? 8192) * 4);
        if (prompt.Length <= limit)
        {
            return prompt;
        }

        var lines = prompt.Split(Environment.NewLine);
        var kept = new List<string>();
        var importSeen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var inSnippet = false;
        var snippetLines = 0;

        foreach (var line in lines)
        {
            if (line.StartsWith("User Request:", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("You are ", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("Mode:", StringComparison.OrdinalIgnoreCase))
            {
                kept.Add(line);
                continue;
            }

            if (line.Trim().Equals("snippet:", StringComparison.OrdinalIgnoreCase))
            {
                inSnippet = true;
                snippetLines = 0;
                kept.Add(line);
                continue;
            }

            if (line.StartsWith("- ", StringComparison.Ordinal))
            {
                inSnippet = false;
            }

            if (inSnippet)
            {
                if (snippetLines++ < 24)
                {
                    kept.Add(line);
                }

                continue;
            }

            if (line.TrimStart().StartsWith("import:", StringComparison.OrdinalIgnoreCase) &&
                !importSeen.Add(line.Trim()))
            {
                continue;
            }

            kept.Add(line);
            if (string.Join(Environment.NewLine, kept).Length >= limit)
            {
                break;
            }
        }

        var compressed = string.Join(Environment.NewLine, kept);
        return compressed.Length <= limit ? compressed : compressed[..limit];
    }
}
