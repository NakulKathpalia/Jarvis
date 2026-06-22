using Jarvis.Models;

namespace Jarvis.Memory;

public sealed record MemorySearchCriteria(
    string Query = "",
    string? Category = null,
    string? Tag = null,
    int? MinImportance = null,
    int? MinConfidence = null,
    MemoryType? MemoryType = null,
    MemoryReviewStatus? ReviewStatus = null,
    bool IncludeExpired = false);
