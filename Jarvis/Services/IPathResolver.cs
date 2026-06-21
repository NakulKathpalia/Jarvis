namespace Jarvis.Services;

public interface IPathResolver
{
    string AppDataDirectory { get; }
    string MemoryDirectory { get; }
    string LogsDirectory { get; }
    string SecurityLogsDirectory { get; }
    string ScreenshotDirectory { get; }
    string GeneratedAudioDirectory { get; }
    string VoiceHistoryPath { get; }
    string MemoryPath { get; }
    string ChatHistoryPath { get; }
    string ChatSessionsPath { get; }
    string CommandLogPath { get; }
    string InteractionLogPath { get; }
    string SecurityAuditLogPath { get; }
    string SettingsPath { get; }
}
