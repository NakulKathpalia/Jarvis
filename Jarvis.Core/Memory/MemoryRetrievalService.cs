using Jarvis.Models;

namespace Jarvis.Memory;

public sealed class MemoryRetrievalService
{
    private readonly MemoryService _memoryService;
    private readonly MemoryRankingService _rankingService;

    public MemoryRetrievalService(
        MemoryService memoryService,
        MemoryRankingService? rankingService = null)
    {
        _memoryService = memoryService;
        _rankingService = rankingService ?? new MemoryRankingService();
    }

    public IReadOnlyList<MemoryRetrievalResult> Retrieve(string query, MemoryRetrievalOptions options)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var now = DateTime.UtcNow;
        return _memoryService.Items
            .Where(item => IsEligible(item, now, options))
            .Select(item => _rankingService.Score(item, query, now))
            .Where(result => result.Score > 0)
            .OrderByDescending(result => result.Score)
            .ThenByDescending(result => result.Memory.Importance)
            .ThenByDescending(result => result.Memory.Confidence)
            .ThenByDescending(result => result.Memory.UpdatedAtUtc)
            .Take(options.ClampedMaxResults)
            .ToList();
    }

    private static bool IsEligible(MemoryItem item, DateTime nowUtc, MemoryRetrievalOptions options)
    {
        if (item.ReviewStatus == MemoryReviewStatus.Rejected || item.ReviewStatus == MemoryReviewStatus.Pending)
        {
            return false;
        }

        if (item.MemoryType == MemoryType.SuggestedMemory &&
            (!options.UseSuggestedMemories || item.ReviewStatus != MemoryReviewStatus.Approved))
        {
            return false;
        }

        if (item.MemoryType == MemoryType.TemporaryContext)
        {
            if (!options.UseTemporaryContext)
            {
                return false;
            }

            if (item.ExpiresAtUtc.HasValue && item.ExpiresAtUtc.Value <= nowUtc)
            {
                return false;
            }
        }

        return item.ReviewStatus == MemoryReviewStatus.Approved;
    }
}
