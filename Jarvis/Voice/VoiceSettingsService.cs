using Jarvis.Models;
using Jarvis.Services;

namespace Jarvis.Voice;

public sealed class VoiceSettingsService
{
    private readonly SettingsService _settingsService;

    public VoiceSettingsService(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public AppSettings Current => _settingsService.Current;

    public VoiceMode Mode => Current.VoiceMode;

    public string Language => string.IsNullOrWhiteSpace(Current.VoiceLanguage)
        ? NormalizeLegacyLanguage(Current.WhisperLanguage)
        : NormalizeLegacyLanguage(Current.VoiceLanguage);

    public bool PushToTalkEnabled => Current.VoiceMode == VoiceMode.PushToTalk;

    public bool NoiseSuppressionEnabled => Current.NoiseSuppression;

    public bool IsConfigured =>
        File.Exists(Current.WhisperExecutablePath)
        && !string.IsNullOrWhiteSpace(Current.WhisperModelPath);

    public string StatusMessage
    {
        get
        {
            if (Current.VoiceMode != VoiceMode.PushToTalk)
            {
                return $"{Current.VoiceMode} is reserved for a future sprint. PushToTalk is currently implemented.";
            }

            if (string.IsNullOrWhiteSpace(Current.WhisperExecutablePath))
            {
                return "Faster-Whisper executable path is not configured.";
            }

            if (!File.Exists(Current.WhisperExecutablePath))
            {
                return "Faster-Whisper executable was not found.";
            }

            if (string.IsNullOrWhiteSpace(Current.WhisperModelPath))
            {
                return "Faster-Whisper model path or model id is not configured.";
            }

            return "PushToTalk speech-to-text is configured.";
        }
    }

    public static string NormalizeLegacyLanguage(string language)
    {
        if (string.IsNullOrWhiteSpace(language) || language.Equals("auto", StringComparison.OrdinalIgnoreCase))
        {
            return "auto";
        }

        return language.Trim().ToLowerInvariant() switch
        {
            "english" => "en",
            "en-us" => "en",
            "en-in" => "en",
            "hindi" => "hi",
            "hinglish" => "auto",
            _ => language.Trim().ToLowerInvariant()
        };
    }
}
