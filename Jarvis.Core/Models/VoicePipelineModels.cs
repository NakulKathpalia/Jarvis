namespace Jarvis.Models;

public enum VoiceMode
{
    PushToTalk,
    WakeWord,
    AlwaysListening,
    Hybrid
}

public enum WakeWordMode
{
    Disabled,
    PushToTalk,
    WakeWord,
    AlwaysListening
}

public sealed class WakeWordConfiguration
{
    public WakeWordMode Mode { get; set; } = WakeWordMode.PushToTalk;
    public string Phrase { get; set; } = "jarvis";
    public string DetectorPath { get; set; } = string.Empty;
    public string ModelPath { get; set; } = string.Empty;
    public bool BackgroundMonitoringEnabled { get; set; }
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}

public interface IWakeWordService
{
    WakeWordConfiguration GetConfiguration();
    WakeWordDetectionResult CheckTranscript(string transcript);
}

public sealed record WakeWordDetectionResult(
    bool Detected,
    string Message,
    string Transcript,
    string Phrase);

public enum VoicePipelineState
{
    Idle,
    Listening,
    Recording,
    Processing,
    Transcribing,
    Understanding,
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
    string? ConfirmationId = null,
    string VoiceSessionId = "",
    DateTime? StartedAtUtc = null,
    DateTime? EndedAtUtc = null,
    long AudioSizeBytes = 0,
    long RecordingDurationMs = 0,
    long ProcessingDurationMs = 0,
    long SttDurationMs = 0,
    long CommandDurationMs = 0,
    long SpeechDurationMs = 0,
    string FailureReason = "",
    string LastCompletedStage = "",
    string SttDevice = "",
    string SpokenResponse = "",
    string TtsProvider = "",
    string VoiceUsed = "",
    bool PlaybackReady = false,
    string PlaybackFailureReason = "",
    bool CommandExecuted = false);

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
    public string Command { get; set; } = string.Empty;
    public string AssistantResponse { get; set; } = string.Empty;
    public string SpokenResponse { get; set; } = string.Empty;
    public VoicePipelineState State { get; set; } = VoicePipelineState.Idle;
    public bool Success { get; set; }
    public bool CommandDetected { get; set; }
    public bool CommandExecuted { get; set; }
    public long ProcessingDurationMs { get; set; }
    public long SttDurationMs { get; set; }
    public long CommandDurationMs { get; set; }
    public long SpeechDurationMs { get; set; }
    public string TtsProvider { get; set; } = string.Empty;
    public string VoiceUsed { get; set; } = string.Empty;
    public string FailureReason { get; set; } = string.Empty;
}

public sealed record VoicePipelineStatus(
    VoicePipelineState State,
    DateTime UpdatedAtUtc,
    string LastTranscript,
    string LastAiResponse,
    string Message,
    string VoiceSessionId = "",
    DateTime? StartedAtUtc = null,
    DateTime? EndedAtUtc = null,
    long AudioSizeBytes = 0,
    long RecordingDurationMs = 0,
    long ProcessingDurationMs = 0,
    long SttDurationMs = 0,
    long CommandDurationMs = 0,
    long SpeechDurationMs = 0,
    bool CommandDetected = false,
    bool CommandExecuted = false,
    string CommandName = "",
    string ErrorDetails = "",
    string LastCompletedStage = "",
    string MicrophoneStatus = "Browser controlled",
    string SttDevice = "",
    string SpokenResponse = "",
    string TtsProvider = "",
    string VoiceUsed = "",
    bool PlaybackReady = false,
    string PlaybackFailureReason = "");

public sealed record VoicePipelineRequest(bool RequireWakeWord = false);

public sealed record VoiceConfirmationRequest(string ConfirmationId);
