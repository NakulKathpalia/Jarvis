namespace Jarvis.Security;

public static class PermissionGroups
{
    public static IReadOnlySet<string> ChatUser { get; } = ToSet(
        PermissionDefinitions.ChatRead,
        PermissionDefinitions.ChatWrite,
        PermissionDefinitions.MemoryRead,
        PermissionDefinitions.MemoryWrite);

    public static IReadOnlySet<string> LocalTools { get; } = ToSet(
        PermissionDefinitions.CommandsExecute,
        PermissionDefinitions.FilesRead,
        PermissionDefinitions.VoiceUse);

    public static IReadOnlySet<string> ConnectedAppsOwn { get; } = ToSet(
        PermissionDefinitions.ConnectorUse,
        PermissionDefinitions.ConnectorManageOwn,
        PermissionDefinitions.OAuthManageOwn);

    public static IReadOnlySet<string> Admin { get; } = ToSet(
        PermissionDefinitions.AdminAccess,
        PermissionDefinitions.AuditReadAll,
        PermissionDefinitions.SettingsWriteSystem,
        PermissionDefinitions.ConnectorManageAll,
        PermissionDefinitions.OAuthManageAll,
        PermissionDefinitions.CommandsExecuteDestructive);

    public static IReadOnlySet<string> Owner { get; } = PermissionDefinitions.All
        .Select(definition => definition.Key)
        .ToHashSet(StringComparer.Ordinal);

    private static IReadOnlySet<string> ToSet(params string[] permissions) =>
        permissions.ToHashSet(StringComparer.Ordinal);
}
