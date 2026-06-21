using Jarvis.Users;

namespace Jarvis.Security;

public static class RoleDefinitions
{
    public static IReadOnlyList<RoleDefinition> All { get; } =
    [
        Create(UserRole.Owner, "Local or server owner with full platform control.", PermissionGroups.Owner),
        Create(UserRole.Admin, "Administrator for users, settings, audit, OAuth, and connectors.",
            PermissionGroups.ChatUser
                .Concat(PermissionGroups.LocalTools)
                .Concat(PermissionGroups.ConnectedAppsOwn)
                .Concat(PermissionGroups.Admin)),
        Create(UserRole.PowerUser, "Advanced user allowed to use local tools and manage own connections.",
            PermissionGroups.ChatUser
                .Concat(PermissionGroups.LocalTools)
                .Concat(PermissionGroups.ConnectedAppsOwn)
                .Append(PermissionDefinitions.SettingsRead)
                .Append(PermissionDefinitions.SettingsWriteOwn)),
        Create(UserRole.User, "Standard user for chat, memory, voice, and non-destructive local tools.",
            PermissionGroups.ChatUser
                .Concat(PermissionGroups.LocalTools)
                .Append(PermissionDefinitions.SettingsRead)),
        Create(UserRole.Guest, "Restricted temporary user.",
            new[]
            {
                PermissionDefinitions.ChatRead,
                PermissionDefinitions.ChatWrite,
                PermissionDefinitions.MemoryRead,
                PermissionDefinitions.SettingsRead
            })
    ];

    public static RoleDefinition Resolve(UserRole role) =>
        All.FirstOrDefault(definition => definition.Role == role)
        ?? All.First(definition => definition.Role == UserRole.Guest);

    private static RoleDefinition Create(
        UserRole role,
        string description,
        IEnumerable<string> allowedPermissions,
        IEnumerable<string>? deniedPermissions = null) =>
        new()
        {
            Role = role,
            Description = description,
            AllowedPermissions = allowedPermissions.Distinct(StringComparer.Ordinal).ToList(),
            DeniedPermissions = (deniedPermissions ?? []).Distinct(StringComparer.Ordinal).ToList()
        };
}
