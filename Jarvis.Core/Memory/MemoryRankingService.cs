using System.Text.RegularExpressions;
using Jarvis.Models;

namespace Jarvis.Memory;

public sealed class MemoryRankingService
{
    private static readonly Regex TokenRegex = new(@"[a-zA-Z0-9]+", RegexOptions.Compiled);
    private static readonly Dictionary<string, string[]> CategoryHints = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Identity"] = ["name", "who", "identity", "about", "personal"],
        ["Preferences"] = ["prefer", "preference", "like", "choice", "recommend", "buy", "should"],
        ["Projects"] = ["project", "jarvis", "build", "code", "repo", "app", "assistant"],
        ["Devices"] = ["device", "laptop", "phone", "mobile", "iphone", "android", "apple", "windows", "pc", "computer"],
        ["Education"] = ["study", "learn", "college", "school", "course", "exam", "education"],
        ["Work"] = ["work", "career", "job", "office", "business", "client"],
        ["Astrology"] = ["astrology", "chart", "planet", "zodiac", "kundli", "horoscope"],
        ["Tarot"] = ["tarot", "card", "reading", "spread"],
        ["Occult"] = ["occult", "spiritual", "ritual", "mystic", "symbol"],
        ["Goals"] = ["goal", "plan", "future", "improve", "habit", "target"],
        ["Health"] = ["health", "sleep", "diet", "exercise", "medical", "fitness"],
        ["General"] = ["general", "remember", "memory"]
    };

    public MemoryRetrievalResult Score(MemoryItem item, string query, DateTime nowUtc)
    {
        var queryTokens = Tokenize(query);
        var itemTokens = Tokenize($"{item.Text} {item.Category} {string.Join(' ', item.Tags)} {item.Source}");
        var matchedTerms = queryTokens
            .Where(token => itemTokens.Contains(token, StringComparer.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var categoryMatches = GetRelevantCategories(queryTokens)
            .Where(category => item.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var score = 0d;
        score += matchedTerms.Count * 10;
        score += queryTokens.Any(token => item.Text.Contains(token, StringComparison.OrdinalIgnoreCase)) ? 8 : 0;
        score += categoryMatches.Count * 14;
        score += item.Importance * 1.5;
        score += item.Confidence * 1.2;
        score += RecencyBoost(item, nowUtc);

        if (item.ReviewStatus == MemoryReviewStatus.Approved)
        {
            score += 5;
        }

        score += item.MemoryType switch
        {
            MemoryType.PermanentMemory => 4,
            MemoryType.TemporaryContext => 3,
            MemoryType.SuggestedMemory => -1,
            _ => 0
        };

        return new MemoryRetrievalResult(item, score, matchedTerms, categoryMatches);
    }

    public IReadOnlySet<string> GetRelevantCategories(string query)
    {
        var queryTokens = Tokenize(query);
        return GetRelevantCategories(queryTokens);
    }

    private static IReadOnlySet<string> GetRelevantCategories(IReadOnlyCollection<string> queryTokens)
    {
        var categories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (category, hints) in CategoryHints)
        {
            if (hints.Any(hint => queryTokens.Contains(hint, StringComparer.OrdinalIgnoreCase)))
            {
                categories.Add(category);
            }
        }

        if (categories.Count == 0)
        {
            categories.Add("General");
        }

        return categories;
    }

    private static double RecencyBoost(MemoryItem item, DateTime nowUtc)
    {
        var age = nowUtc - item.UpdatedAtUtc;
        if (age.TotalDays <= 1)
        {
            return 8;
        }

        if (age.TotalDays <= 7)
        {
            return 5;
        }

        if (age.TotalDays <= 30)
        {
            return 2;
        }

        return 0;
    }

    private static IReadOnlyList<string> Tokenize(string value)
    {
        return TokenRegex.Matches(value.ToLowerInvariant())
            .Select(match => match.Value)
            .Where(token => token.Length >= 3)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
