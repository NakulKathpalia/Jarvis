namespace Jarvis.Services;

public sealed class PathResolver : IPathResolver
{
    private readonly string _basePath;
    private readonly string _webRootPath;

    public PathResolver(string basePath, string? webRootPath = null)
    {
        _basePath = basePath;
        _webRootPath = string.IsNullOrWhiteSpace(webRootPath)
            ? Path.Combine(_basePath, "wwwroot")
            : webRootPath;
        Directory.CreateDirectory(AppDataDirectory);
        Directory.CreateDirectory(MemoryDirectory);
        Directory.CreateDirectory(LogsDirectory);
        Directory.CreateDirectory(SecurityLogsDirectory);
        Directory.CreateDirectory(ScreenshotDirectory);
        Directory.CreateDirectory(GeneratedAudioDirectory);
        Directory.CreateDirectory(IngestionUploadDirectory);
    }

    public string AppDataDirectory => Path.Combine(_basePath, "Data");
    public string MemoryDirectory => Path.Combine(_basePath, "Memory");
    public string LogsDirectory => Path.Combine(AppDataDirectory, "logs");
    public string SecurityLogsDirectory => Path.Combine(_basePath, "logs");
    public string ScreenshotDirectory => Path.Combine(AppDataDirectory, "screenshots");
    public string GeneratedAudioDirectory => Path.Combine(_webRootPath, "generated-audio");
    public string IngestionUploadDirectory => Path.Combine(AppDataDirectory, "uploads", "ingestion");
    public string VoiceHistoryPath => Path.Combine(AppDataDirectory, "voice_history.json");
    public string IngestionJobsPath => Path.Combine(AppDataDirectory, "ingestion_jobs.json");
    public string KnowledgePath => Path.Combine(AppDataDirectory, "knowledge.json");
    public string MemoryPath => Path.Combine(MemoryDirectory, "memory.json");
    public string ChatHistoryPath => Path.Combine(AppDataDirectory, "chat_history.json");
    public string ChatSessionsPath => Path.Combine(AppDataDirectory, "chat_sessions.json");
    public string CommandLogPath => Path.Combine(AppDataDirectory, "command_logs.json");
    public string InteractionLogPath => Path.Combine(AppDataDirectory, "interaction_logs.json");
    public string SecurityAuditLogPath => Path.Combine(SecurityLogsDirectory, "security-audit.log");
    public string SettingsPath => Path.Combine(_basePath, "appsettings.json");
}
