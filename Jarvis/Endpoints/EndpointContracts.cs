using Jarvis.Models;

namespace Jarvis.Endpoints;

public sealed record ChatRequest(string Message);
public sealed record ChatSessionCreateRequest(string? Title);
public sealed record ChatSessionMessageRequest(string Message);
public sealed record MemoryRequest(
    string Text,
    string? Category,
    string[]? Tags = null,
    int Importance = 3,
    int Confidence = 10,
    string? Source = "Manual",
    MemoryType MemoryType = MemoryType.PermanentMemory,
    MemoryReviewStatus? ReviewStatus = null,
    DateTime? ExpiresAtUtc = null);
public sealed record MemoryUpdateRequest(
    string Text,
    string? Category,
    string[]? Tags = null,
    int? Importance = null,
    int? Confidence = null,
    string? Source = null,
    MemoryType? MemoryType = null,
    MemoryReviewStatus? ReviewStatus = null,
    DateTime? ExpiresAtUtc = null);
public sealed record MemoryRetrieveRequest(string Query, int? MaxResults = null);
public sealed record BulkMemoryRequest(
    string[] MemoryIds,
    string? Category = null,
    int? Importance = null,
    int? Confidence = null,
    bool ConvertSuggestedToPermanent = false);
public sealed record IngestionCandidateUpdateRequest(
    string? Content,
    string? Category,
    MemoryType? MemoryType,
    int? Importance,
    int? Confidence);
public sealed record BulkIngestionCandidateRequest(
    string[] CandidateIds,
    string? Category = null,
    int? Importance = null,
    int? Confidence = null);
public sealed record SaveIngestionAsKnowledgeRequest(
    string? Title,
    KnowledgeCategory Category = KnowledgeCategory.General);
public sealed record SaveIngestionAsMemoryRequest(
    string? Text = null,
    string? Category = "General",
    MemoryType MemoryType = MemoryType.PermanentMemory,
    int Importance = 3,
    int Confidence = 7);
public sealed record KnowledgeCreateRequest(
    string Title,
    string Content,
    KnowledgeCategory Category = KnowledgeCategory.General,
    string? Source = "Manual",
    string? SourceFile = null,
    string? SourceType = null);
public sealed record SpeakRequest(string Text);
public sealed record VoiceCommandRequest(string Transcript, bool Confirmed = false);
public sealed record WakeWordCheckRequest(string Transcript);
public sealed record FileOpenRequest(string Path);
