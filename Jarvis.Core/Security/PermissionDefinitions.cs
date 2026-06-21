namespace Jarvis.Security;

public static class PermissionDefinitions
{
    public const string MemoryRead = "Memory.Read";
    public const string MemoryWrite = "Memory.Write";
    public const string ChatRead = "Chat.Read";
    public const string ChatWrite = "Chat.Write";
    public const string CommandsExecute = "Commands.Execute";
    public const string CommandsExecuteDestructive = "Commands.Execute.Destructive";
    public const string FilesRead = "Files.Read";
    public const string FilesWrite = "Files.Write";
    public const string VoiceUse = "Voice.Use";
    public const string OAuthManageOwn = "OAuth.ManageOwn";
    public const string OAuthManageAll = "OAuth.ManageAll";
    public const string ConnectorUse = "Connector.Use";
    public const string ConnectorManageOwn = "Connector.ManageOwn";
    public const string ConnectorManageAll = "Connector.ManageAll";
    public const string AdminAccess = "Admin.Access";
    public const string AuditReadOwn = "Audit.ReadOwn";
    public const string AuditReadAll = "Audit.ReadAll";
    public const string SettingsRead = "Settings.Read";
    public const string SettingsWriteOwn = "Settings.WriteOwn";
    public const string SettingsWriteSystem = "Settings.WriteSystem";

    public static IReadOnlyList<PermissionDefinition> All { get; } =
    [
        new(MemoryRead, "Read owned memories.", "Memory"),
        new(MemoryWrite, "Create, update, and delete owned memories.", "Memory"),
        new(ChatRead, "Read owned chat sessions.", "Chat"),
        new(ChatWrite, "Create and update owned chat sessions.", "Chat"),
        new(CommandsExecute, "Execute local assistant commands.", "Commands", SecurityRiskLevel.Medium),
        new(CommandsExecuteDestructive, "Execute destructive local commands after confirmation.", "Commands", SecurityRiskLevel.Dangerous),
        new(FilesRead, "Search and open indexed local files.", "Files", SecurityRiskLevel.Medium),
        new(FilesWrite, "Write or modify local files.", "Files", SecurityRiskLevel.Dangerous),
        new(VoiceUse, "Use voice transcription, wake word, speech, and voice commands.", "Voice"),
        new(OAuthManageOwn, "Manage the user's own OAuth connections.", "OAuth", SecurityRiskLevel.Medium),
        new(OAuthManageAll, "Manage OAuth connections for all users.", "OAuth", SecurityRiskLevel.Dangerous),
        new(ConnectorUse, "Use enabled connected app capabilities.", "Connectors", SecurityRiskLevel.Medium),
        new(ConnectorManageOwn, "Manage the user's own connected apps.", "Connectors", SecurityRiskLevel.Medium),
        new(ConnectorManageAll, "Manage connected apps for all users.", "Connectors", SecurityRiskLevel.Dangerous),
        new(AdminAccess, "Access administrative platform features.", "Admin", SecurityRiskLevel.Dangerous),
        new(AuditReadOwn, "Read the user's own audit events.", "Audit", SecurityRiskLevel.Medium),
        new(AuditReadAll, "Read all users' audit events.", "Audit", SecurityRiskLevel.Dangerous),
        new(SettingsRead, "Read application settings.", "Settings"),
        new(SettingsWriteOwn, "Update user-owned settings.", "Settings", SecurityRiskLevel.Medium),
        new(SettingsWriteSystem, "Update system-wide settings.", "Settings", SecurityRiskLevel.Dangerous)
    ];

    public static bool Exists(string permission) =>
        All.Any(definition => string.Equals(definition.Key, permission, StringComparison.Ordinal));
}
