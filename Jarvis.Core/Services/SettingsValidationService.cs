using Jarvis.Models;

namespace Jarvis.Services;

public sealed class SettingsValidationService
{
    private readonly SettingsService _settingsService;

    public SettingsValidationService(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public SettingsValidationResult Validate()
    {
        var warnings = new List<string>();
        var settings = _settingsService.Current;

        if (!Uri.TryCreate(settings.OllamaBaseUrl, UriKind.Absolute, out _))
        {
            warnings.Add("Ollama URL is not a valid absolute URL.");
        }

        AddFileWarning(warnings, settings.WhisperExecutablePath, "Whisper executable");
        AddFileWarning(warnings, settings.WhisperModelPath, "Whisper model");
        AddFileWarning(warnings, settings.PiperExecutablePath, "Piper executable");
        AddFileWarning(warnings, settings.PiperModelPath, "Piper model");

        return new SettingsValidationResult(warnings);
    }

    private static void AddFileWarning(List<string> warnings, string path, string label)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            warnings.Add($"{label} path is not configured.");
            return;
        }

        if (!File.Exists(path))
        {
            warnings.Add($"{label} was not found: {path}");
        }
    }
}
