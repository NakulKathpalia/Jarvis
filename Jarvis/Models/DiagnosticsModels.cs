namespace Jarvis.Models;

public sealed record SettingsValidationResult(IReadOnlyCollection<string> Warnings);

public sealed record DiagnosticsResult(
    string Platform,
    string AppDataPath,
    string MemoryPath,
    string LogsPath,
    string ScreenshotPath,
    string GeneratedAudioPath,
    ServiceDiagnostic Ollama,
    ServiceDiagnostic Whisper,
    ServiceDiagnostic Piper,
    IReadOnlyCollection<string> Warnings);

public sealed record ServiceDiagnostic(
    bool Healthy,
    string Message);
