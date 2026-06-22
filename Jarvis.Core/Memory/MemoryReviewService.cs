using Jarvis.Models;

namespace Jarvis.Memory;

public sealed class MemoryReviewService
{
    public bool IsPending(MemoryItem item) =>
        item.MemoryType == MemoryType.SuggestedMemory
        && item.ReviewStatus == MemoryReviewStatus.Pending;

    public MemoryItem Approve(MemoryItem item)
    {
        item.MemoryType = MemoryType.PermanentMemory;
        item.ReviewStatus = MemoryReviewStatus.Approved;
        item.UpdatedAtUtc = DateTime.UtcNow;
        return item;
    }

    public MemoryItem Reject(MemoryItem item)
    {
        item.ReviewStatus = MemoryReviewStatus.Rejected;
        item.UpdatedAtUtc = DateTime.UtcNow;
        return item;
    }
}
