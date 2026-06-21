namespace Jarvis.Security;

public sealed record ResolvedPermissions(
    IReadOnlySet<string> Allowed,
    IReadOnlySet<string> Denied);
