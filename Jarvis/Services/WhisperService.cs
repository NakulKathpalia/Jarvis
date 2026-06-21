using Jarvis.Voice;

namespace Jarvis.Services;

public sealed class WhisperService
{
    private readonly SettingsService _settingsService;
    private readonly SpeechToTextService? _speechToTextService;

    public WhisperService(SettingsService settingsService, SpeechToTextService? speechToTextService = null)
    {
        _settingsService = settingsService;
        _speechToTextService = speechToTextService;
    }

    public bool IsConfigured =>
        _speechToTextService?.IsConfigured
        ?? (File.Exists(_settingsService.Current.WhisperExecutablePath)
            && !string.IsNullOrWhiteSpace(_settingsService.Current.WhisperModelPath));

    public string StatusMessage
    {
        get
        {
            if (_speechToTextService is not null)
            {
                return _speechToTextService.StatusMessage;
            }

            if (string.IsNullOrWhiteSpace(_settingsService.Current.WhisperExecutablePath))
            {
                return "Faster-Whisper executable path is not configured.";
            }

            if (!File.Exists(_settingsService.Current.WhisperExecutablePath))
            {
                return "Faster-Whisper executable was not found.";
            }

            if (string.IsNullOrWhiteSpace(_settingsService.Current.WhisperModelPath))
            {
                return "Faster-Whisper model path or model id is not configured.";
            }

            return "PushToTalk speech-to-text is configured.";
        }
    }

    public async Task<WhisperTranscriptionResult> TranscribeAsync(
        IFormFile audio,
        CancellationToken cancellationToken = default)
    {
        var speechToTextService = _speechToTextService
            ?? new SpeechToTextService(new VoiceSettingsService(_settingsService));
        var result = await speechToTextService.TranscribeAsync(audio, cancellationToken);
        return new WhisperTranscriptionResult(
            result.IsReady,
            result.Succeeded,
            result.Transcript,
            result.Message);
    }
}

public sealed record WhisperTranscriptionResult(
    bool IsReady,
    bool Succeeded,
    string Transcript,
    string Message)
{
    public static WhisperTranscriptionResult Success(string transcript) =>
        new(true, true, transcript, "Transcription complete.");

    public static WhisperTranscriptionResult NotReady(string message) =>
        new(false, false, string.Empty, message);

    public static WhisperTranscriptionResult Failed(string message) =>
        new(true, false, string.Empty, message);
}
