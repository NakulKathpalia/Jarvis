namespace Jarvis.Auth;

public sealed record AuthUser(
    string Id,
    string Email,
    string Name,
    AuthProviderType Provider);
