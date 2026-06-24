using Jarvis.Models;

namespace Jarvis.Ingestion;

public sealed class MemoryCandidateService
{
    private static readonly (string Category, string[] Keywords)[] CategoryRules =
    [
        ("Astrology", ["astrology", "zodiac", "planet", "mars", "venus", "saturn", "rahu", "ketu", "birth chart", "nakshatra", "lagna"]),
        ("Tarot", ["tarot", "card", "major arcana", "minor arcana", "spread", "cups", "wands", "swords", "pentacles"]),
        ("Occult", ["occult", "ritual", "symbol", "sigil", "alchemy", "esoteric", "mantra"]),
        ("Vastu", ["vastu", "direction", "north east", "south west", "entrance", "kitchen", "mandir"]),
        ("Projects", ["project", "roadmap", "milestone", "sprint", "todo"]),
        ("Preferences", ["prefer", "preference", "like", "dislike", "favorite"]),
        ("Identity", ["i am", "my name", "birthday", "born", "identity"]),
        ("Education", ["study", "course", "exam", "college", "learn"]),
        ("Work", ["work", "job", "client", "meeting", "office"]),
        ("Goals", ["goal", "plan", "target", "habit", "routine"]),
        ("Health", ["health", "sleep", "diet", "exercise", "doctor"])
    ];

    public IReadOnlyList<IngestionMemoryCandidate> CreateCandidates(IngestionJob job)
    {
        var text = job.ExtractedText.Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        var chunks = SplitIntoChunks(text)
            .Take(12)
            .Select((chunk, index) => CreateCandidate(job, chunk, index))
            .ToList();

        return chunks;
    }

    private static IngestionMemoryCandidate CreateCandidate(IngestionJob job, string content, int index)
    {
        var category = SuggestCategory(content);
        return new IngestionMemoryCandidate
        {
            Id = Guid.NewGuid().ToString("N"),
            UserId = job.UserId,
            Content = content,
            SuggestedCategory = category,
            SuggestedMemoryType = MemoryType.SuggestedMemory,
            SuggestedImportance = category is "Astrology" or "Tarot" or "Occult" or "Vastu" ? 5 : 3,
            SuggestedConfidence = job.SourceType == IngestionSourceType.Pdf ? 7 : 5,
            SourceFile = job.FileName,
            SourcePage = job.TextBlocks.FirstOrDefault()?.PageNumber,
            SourceTextRange = $"block-{index + 1}",
            ReviewStatus = MemoryReviewStatus.Pending
        };
    }

    private static IEnumerable<string> SplitIntoChunks(string text)
    {
        var normalized = text.Replace("\r\n", "\n").Replace('\r', '\n');
        var paragraphs = normalized.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(paragraph => paragraph.Length >= 12)
            .ToList();

        if (paragraphs.Count > 0)
        {
            foreach (var paragraph in paragraphs)
            {
                foreach (var chunk in SplitLongText(paragraph))
                {
                    yield return chunk;
                }
            }

            yield break;
        }

        foreach (var chunk in SplitLongText(normalized))
        {
            yield return chunk;
        }
    }

    private static IEnumerable<string> SplitLongText(string text)
    {
        const int maxLength = 420;
        var remaining = text.Trim();
        while (remaining.Length > maxLength)
        {
            var splitAt = remaining.LastIndexOfAny(['.', ';', ','], maxLength);
            splitAt = splitAt < 120 ? maxLength : splitAt + 1;
            yield return remaining[..splitAt].Trim();
            remaining = remaining[splitAt..].Trim();
        }

        if (!string.IsNullOrWhiteSpace(remaining))
        {
            yield return remaining;
        }
    }

    private static string SuggestCategory(string content)
    {
        var normalized = content.ToLowerInvariant();
        foreach (var (category, keywords) in CategoryRules)
        {
            if (keywords.Any(keyword => normalized.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            {
                return category;
            }
        }

        return "General";
    }
}
