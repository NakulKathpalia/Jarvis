namespace Jarvis.Models;

public enum VoicePipelineState
{
    Idle,
    Listening,
    Recording,
    Transcribing,
    WakeWordChecking,
    CommandDetected,
    AwaitingConfirmation,
    ExecutingCommand,
    GeneratingAIResponse,
    Speaking,
    Completed,
    Error
}

public sealed record VoicePipelineResult(
    string Transcript,
    bool WakeWordDetected,
    bool CommandDetected,
    string CommandName,
    bool RequiresConfirmation,
    string AiResponse,
    string AudioUrl,
    VoicePipelineState State,
    bool Success,
    string Message,
    string? ConfirmationId = null);

public sealed class VoiceHistoryItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string DeviceId { get; set; } = Environment.MachineName;
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public string Transcript { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public VoicePipelineState State { get; set; } = VoicePipelineState.Idle;
    public bool Success { get; set; }
}

public sealed record VoicePipelineStatus(
    VoicePipelineState State,
    DateTime UpdatedAtUtc,
    string LastTranscript,
    string LastAiResponse,
    string Message);

public sealed record VoicePipelineRequest(bool RequireWakeWord = false);

public sealed record VoiceConfirmationRequest(string ConfirmationId);
