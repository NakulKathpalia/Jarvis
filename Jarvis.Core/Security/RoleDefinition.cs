using Jarvis.Users;

namespace Jarvis.Security;

public sealed class RoleDefinition
{
    public UserRole Role { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> AllowedPermissions { get; set; } = [];
    public List<string> DeniedPermissions { get; set; } = [];
}
