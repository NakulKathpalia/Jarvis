namespace Jarvis.Models;

public enum KnowledgeCategory
{
    Astrology,
    Tarot,
    Occult,
    Vastu,
    Research,
    Books,
    Documents,
    General
}

public sealed class KnowledgeItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public KnowledgeCategory Category { get; set; } = KnowledgeCategory.General;
    public string Source { get; set; } = "Manual";
    public string SourceFile { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public int CharacterCount { get; set; }
    public int WordCount { get; set; }
    public int AccessCount { get; set; }
    public DateTime? LastAccessedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
