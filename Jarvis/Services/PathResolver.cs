namespace Jarvis.Services;

public sealed class PathResolver : IPathResolver
{
    private readonly string _basePath;

    public PathResolver(string basePath)
    {
        _basePath = basePath;
        Directory.CreateDirectory(AppDataDirectory);
        Directory.CreateDirectory(MemoryDirectory);
        Directory.CreateDirectory(LogsDirectory);
        Directory.CreateDirectory(ScreenshotDirectory);
        Directory.CreateDirectory(GeneratedAudioDirectory);
    }

    public string AppDataDirectory => Path.Combine(_basePath, "Data");
    public string MemoryDirectory => Path.Combine(_basePath, "Memory");
    public string LogsDirectory => Path.Combine(AppDataDirectory, "logs");
    public string ScreenshotDirectory => Path.Combine(AppDataDirectory, "screenshots");
    public string GeneratedAudioDirectory => Path.Combine(_basePath, "wwwroot", "generated-audio");
    public string VoiceHistoryPath => Path.Combine(AppDataDirectory, "voice_history.json");
    public string MemoryPath => Path.Combine(MemoryDirectory, "memory.json");
    public string ChatHistoryPath => Path.Combine(AppDataDirectory, "chat_history.json");
    public string CommandLogPath => Path.Combine(AppDataDirectory, "command_logs.json");
    public string SettingsPath => Path.Combine(_basePath, "appsettings.json");
}
