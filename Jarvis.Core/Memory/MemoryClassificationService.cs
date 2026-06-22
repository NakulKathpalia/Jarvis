using Jarvis.Models;

namespace Jarvis.Memory;

public sealed class MemoryClassificationService
{
    public MemoryType NormalizeType(MemoryType? memoryType, string? source = null)
    {
        return memoryType ?? MemoryType.PermanentMemory;
    }

    public MemoryReviewStatus DefaultReviewStatus(MemoryType memoryType)
    {
        return memoryType == MemoryType.SuggestedMemory
            ? MemoryReviewStatus.Pending
            : MemoryReviewStatus.Approved;
    }

    public string NormalizeCategory(string? category) => MemoryCategories.Normalize(category);
}
