using Jarvis.Models;

namespace Jarvis.Memory;

public sealed record MemoryRetrievalResult(
    MemoryItem Memory,
    double Score,
    IReadOnlyList<string> MatchedTerms,
    IReadOnlyList<string> MatchedCategories);
