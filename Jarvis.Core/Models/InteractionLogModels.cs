using System.Text.Json;

namespace Jarvis.Models;

public enum InteractionSource
{
    Chat,
    Voice,
    Control,
    System
}

public enum InteractionType
{
    UserInput,
    VoiceRecording,
    Transcription,
    WakeWordCheck,
    CommandParsing,
    Confirmation,
    CommandExecution,
    AiFallback,
    AiResponse,
    Tts,
    Error,
    SystemStatus
}

public enum InteractionStatus
{
    Started,
    Success,
    Failed,
    Pending,
    Cancelled,
    Skipped
}

public sealed class InteractionLogEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public InteractionSource Source { get; set; } = InteractionSource.System;
    public InteractionType Type { get; set; } = InteractionType.SystemStatus;
    public string Stage { get; set; } = string.Empty;
    public string Input { get; set; } = string.Empty;
    public string Output { get; set; } = string.Empty;
    public InteractionStatus Status { get; set; } = InteractionStatus.Started;
    public string Message { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
    public Dictionary<string, JsonElement> Metadata { get; set; } = [];
}

public sealed record InteractionLogRequest(
    InteractionSource Source,
    InteractionType Type,
    string Stage,
    string? Input,
    string? Output,
    InteractionStatus Status,
    string? Message,
    string? Error,
    Dictionary<string, JsonElement>? Metadata = null);

public sealed record InteractionStatusResult(
    bool BackendConnected,
    InteractionLogEntry? LastAction,
    InteractionLogEntry? LastVoiceTranscript,
    InteractionLogEntry? LastCommandParsed,
    InteractionLogEntry? LastError);
