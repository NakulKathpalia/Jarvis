namespace Jarvis.Migrations;

public sealed record FileStorageMigrationPaths(
    string MemoryPath,
    string ChatHistoryPath,
    string ChatSessionsPath,
    string SettingsPath,
    string InteractionLogPath,
    string CommandLogPath,
    string VoiceHistoryPath);
