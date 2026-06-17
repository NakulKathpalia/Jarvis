namespace Jarvis.Services;

public interface IPathResolver
{
    string AppDataDirectory { get; }
    string MemoryDirectory { get; }
    string LogsDirectory { get; }
    string ScreenshotDirectory { get; }
    string GeneratedAudioDirectory { get; }
    string VoiceHistoryPath { get; }
    string MemoryPath { get; }
    string ChatHistoryPath { get; }
    string ChatSessionsPath { get; }
    string CommandLogPath { get; }
    string SettingsPath { get; }
}
