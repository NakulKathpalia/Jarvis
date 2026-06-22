using Jarvis.Models;

namespace Jarvis.Memory;

public sealed class MemorySearchService
{
    public IReadOnlyList<MemoryItem> Search(IEnumerable<MemoryItem> items, MemorySearchCriteria criteria)
    {
        var normalizedQuery = NormalizeQuery(criteria.Query);
        var normalizedCategory = NormalizeQuery(criteria.Category);
        var normalizedTag = NormalizeQuery(criteria.Tag);
        var now = DateTime.UtcNow;

        return items
            .Where(item =>
            {
                if (!criteria.IncludeExpired && item.ExpiresAtUtc.HasValue && item.ExpiresAtUtc.Value <= now)
                {
                    return false;
                }

                if (criteria.MinImportance.HasValue && item.Importance < criteria.MinImportance.Value)
                {
                    return false;
                }

                if (criteria.MinConfidence.HasValue && item.Confidence < criteria.MinConfidence.Value)
                {
                    return false;
                }

                if (criteria.MemoryType.HasValue && item.MemoryType != criteria.MemoryType.Value)
                {
                    return false;
                }

                if (criteria.ReviewStatus.HasValue && item.ReviewStatus != criteria.ReviewStatus.Value)
                {
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(normalizedCategory) &&
                    !item.Category.Contains(normalizedCategory, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(normalizedTag) &&
                    !item.Tags.Any(itemTag => itemTag.Contains(normalizedTag, StringComparison.OrdinalIgnoreCase)))
                {
                    return false;
                }

                if (string.IsNullOrWhiteSpace(normalizedQuery))
                {
                    return true;
                }

                return item.Text.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)
                    || item.Category.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)
                    || item.Source.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)
                    || item.Tags.Any(itemTag => itemTag.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase));
            })
            .OrderByDescending(item => item.UpdatedAtUtc)
            .ThenByDescending(item => item.Importance)
            .ThenByDescending(item => item.CreatedAtUtc)
            .ToList();
    }

    private static string NormalizeQuery(string? value) => value?.Trim() ?? string.Empty;
}
