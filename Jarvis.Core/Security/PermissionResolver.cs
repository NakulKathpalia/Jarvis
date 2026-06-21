using Jarvis.Users;

namespace Jarvis.Security;

public sealed class PermissionResolver
{
    public ResolvedPermissions Resolve(IEnumerable<UserRole> roles)
    {
        var allowed = new HashSet<string>(StringComparer.Ordinal);
        var denied = new HashSet<string>(StringComparer.Ordinal);

        foreach (var role in roles.DefaultIfEmpty(UserRole.Guest))
        {
            var definition = RoleDefinitions.Resolve(role);
            allowed.UnionWith(definition.AllowedPermissions);
            denied.UnionWith(definition.DeniedPermissions);
        }

        allowed.ExceptWith(denied);
        return new ResolvedPermissions(allowed, denied);
    }
}
