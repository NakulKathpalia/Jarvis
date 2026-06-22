namespace Jarvis.Models;

public enum MemoryType
{
    TemporaryContext,
    SuggestedMemory,
    PermanentMemory
}

public enum MemoryReviewStatus
{
    Pending,
    Approved,
    Rejected
}

public static class MemoryCategories
{
    public static IReadOnlyList<string> Defaults { get; } =
    [
        "Identity",
        "Preferences",
        "Projects",
        "Devices",
        "Education",
        "Work",
        "Astrology",
        "Tarot",
        "Occult",
        "Goals",
        "Health",
        "General"
    ];

    public static string Normalize(string? category)
    {
        return string.IsNullOrWhiteSpace(category) ? "General" : category.Trim();
    }
}

public sealed class MemoryItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public List<string> Tags { get; set; } = [];
    public int Importance { get; set; } = 3;
    public int Confidence { get; set; } = 10;
    public string Source { get; set; } = "Manual";
    public MemoryType MemoryType { get; set; } = MemoryType.PermanentMemory;
    public MemoryReviewStatus ReviewStatus { get; set; } = MemoryReviewStatus.Approved;
    public DateTime? ExpiresAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
