using Jarvis.Models;

namespace Jarvis.Knowledge;

public sealed class KnowledgeSearchService
{
    public IReadOnlyList<KnowledgeItem> Search(
        IEnumerable<KnowledgeItem> items,
        string? query,
        KnowledgeCategory? category = null,
        string? sourceType = null,
        DateTime? importedAfterUtc = null,
        DateTime? importedBeforeUtc = null,
        int limit = 50)
    {
        var terms = (query ?? string.Empty)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return items
            .Where(item => category is null || item.Category == category)
            .Where(item => string.IsNullOrWhiteSpace(sourceType) || item.SourceType.Equals(sourceType, StringComparison.OrdinalIgnoreCase))
            .Where(item => importedAfterUtc is null || item.CreatedAtUtc >= importedAfterUtc)
            .Where(item => importedBeforeUtc is null || item.CreatedAtUtc <= importedBeforeUtc)
            .Select(item => new
            {
                Item = item,
                Score = Score(item, terms)
            })
            .Where(result => terms.Length == 0 || result.Score > 0)
            .OrderByDescending(result => result.Score)
            .ThenByDescending(result => result.Item.UpdatedAtUtc)
            .Take(Math.Clamp(limit, 1, 200))
            .Select(result => result.Item)
            .ToList();
    }

    private static int Score(KnowledgeItem item, IReadOnlyList<string> terms)
    {
        if (terms.Count == 0)
        {
            return 1;
        }

        var text = $"{item.Title} {item.Content} {item.Category} {item.SourceFile}";
        return terms.Count(term => text.Contains(term, StringComparison.OrdinalIgnoreCase));
    }
}
