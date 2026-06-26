using Jarvis.Models;

namespace Jarvis.Ingestion;

public enum IngestionStatus
{
    Uploaded,
    Extracted,
    OcrRequired,
    ExtractionFailed,
    CandidatesGenerated,
    Completed
}

public enum IngestionSourceType
{
    Pdf,
    Image
}

public sealed class IngestionTextBlock
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public int? PageNumber { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Confidence { get; set; } = 10;
    public string Status { get; set; } = "Extracted";
}

public sealed class IngestionMemoryCandidate
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string UserId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string SuggestedCategory { get; set; } = "General";
    public MemoryType SuggestedMemoryType { get; set; } = MemoryType.SuggestedMemory;
    public int SuggestedImportance { get; set; } = 3;
    public int SuggestedConfidence { get; set; } = 6;
    public string SourceFile { get; set; } = string.Empty;
    public int? SourcePage { get; set; }
    public string SourceTextRange { get; set; } = string.Empty;
    public MemoryReviewStatus ReviewStatus { get; set; } = MemoryReviewStatus.Pending;
    public string? ApprovedMemoryId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class IngestionJob
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string UserId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string StoredPath { get; set; } = string.Empty;
    public IngestionStatus Status { get; set; } = IngestionStatus.Uploaded;
    public string ExtractedText { get; set; } = string.Empty;
    public List<IngestionTextBlock> TextBlocks { get; set; } = [];
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public string ErrorMessage { get; set; } = string.Empty;
    public IngestionSourceType SourceType { get; set; }
    public string ExtractionSource { get; set; } = string.Empty;
    public string ExtractionLanguage { get; set; } = string.Empty;
    public int ExtractionConfidence { get; set; }
    public int ExtractedCharacterCount { get; set; }
    public int ExtractedWordCount { get; set; }
    public List<IngestionMemoryCandidate> Candidates { get; set; } = [];
    public List<string> SuggestedMemoryIds { get; set; } = [];
}

public sealed record IngestionExtractionResult(
    bool Succeeded,
    IngestionStatus Status,
    string ExtractedText,
    IReadOnlyList<IngestionTextBlock> TextBlocks,
    string Message,
    string ExtractionSource = "",
    string ExtractionLanguage = "",
    int ExtractionConfidence = 0,
    string ErrorMessage = "");
