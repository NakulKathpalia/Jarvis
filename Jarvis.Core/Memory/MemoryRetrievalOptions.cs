namespace Jarvis.Memory;

public sealed record MemoryRetrievalOptions(
    int MaxResults = 5,
    bool UseTemporaryContext = true,
    bool UseSuggestedMemories = true)
{
    public int ClampedMaxResults => Math.Clamp(MaxResults, 1, 10);
}
