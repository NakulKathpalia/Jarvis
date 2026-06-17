namespace Jarvis.Services;

public sealed class WakeWordService
{
    private readonly SettingsService _settingsService;
    private readonly WhisperService _whisperService;

    public WakeWordService(SettingsService settingsService, WhisperService whisperService)
    {
        _settingsService = settingsService;
        _whisperService = whisperService;
    }

    public bool IsConfigured =>
        _settingsService.Current.WakeWordEnabled
        && !string.IsNullOrWhiteSpace(_settingsService.Current.WakeWordPhrase)
        && (HasExternalDetector || _whisperService.IsConfigured);

    public string Mode =>
        HasExternalDetector ? "external-detector" : _whisperService.IsConfigured ? "local-whisper" : "not-configured";

    public string StatusMessage
    {
        get
        {
            if (!_settingsService.Current.WakeWordEnabled)
            {
                return "Wake word is disabled.";
            }

            if (string.IsNullOrWhiteSpace(_settingsService.Current.WakeWordPhrase))
            {
                return "Wake word phrase is not configured.";
            }

            if (HasExternalDetector)
            {
                return "Wake word is configured.";
            }

            if (_whisperService.IsConfigured)
            {
                return "Wake word is ready with local Whisper phrase detection.";
            }

            return "Wake word needs Whisper or an external detector configured.";
        }
    }

    private bool HasExternalDetector =>
        File.Exists(_settingsService.Current.WakeWordDetectorPath)
        && File.Exists(_settingsService.Current.WakeWordModelPath);

    public WakeWordDetectionResult CheckTranscript(string transcript)
    {
        if (!_settingsService.Current.WakeWordEnabled)
        {
            return new(false, "Wake word is disabled.", transcript, _settingsService.Current.WakeWordPhrase);
        }

        if (!IsConfigured)
        {
            return new(false, StatusMessage, transcript, _settingsService.Current.WakeWordPhrase);
        }

        var phrase = Normalize(_settingsService.Current.WakeWordPhrase);
        var normalizedTranscript = Normalize(transcript);
        var detected = !string.IsNullOrWhiteSpace(phrase)
            && $" {normalizedTranscript} ".Contains($" {phrase} ", StringComparison.OrdinalIgnoreCase);

        return detected
            ? new(true, "Wake word detected.", transcript, _settingsService.Current.WakeWordPhrase)
            : new(false, "Wake word not detected.", transcript, _settingsService.Current.WakeWordPhrase);
    }

    private static string Normalize(string value)
    {
        var chars = value
            .Trim()
            .ToLowerInvariant()
            .Select(character => char.IsLetterOrDigit(character) ? character : ' ')
            .ToArray();

        return string.Join(' ', new string(chars).Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}

public sealed record WakeWordDetectionResult(
    bool Detected,
    string Message,
    string Transcript,
    string Phrase);
