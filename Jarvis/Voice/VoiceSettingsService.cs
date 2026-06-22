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

    public string EngineName => IsWhisperCppConfigured ? "whisper.cpp" : "Faster-Whisper";

    public string PreferredDevice => IsWhisperCppConfigured ? "gpu" : "cuda";

    public string FallbackDevice => "cpu";

    public bool IsWhisperCppConfigured
    {
        get
        {
            var executableName = Path.GetFileNameWithoutExtension(Current.WhisperExecutablePath);
            return executableName.Contains("whisper-cli", StringComparison.OrdinalIgnoreCase)
                || executableName.Contains("main", StringComparison.OrdinalIgnoreCase)
                || Current.WhisperModelPath.EndsWith(".bin", StringComparison.OrdinalIgnoreCase);
        }
    }

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
                return "Speech-to-text executable path is not configured.";
            }

            if (!File.Exists(Current.WhisperExecutablePath))
            {
                return "Speech-to-text executable was not found.";
            }

            if (string.IsNullOrWhiteSpace(Current.WhisperModelPath))
            {
                return "Speech-to-text model path or model id is not configured.";
            }

            return $"PushToTalk speech-to-text is configured with {EngineName}.";
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
