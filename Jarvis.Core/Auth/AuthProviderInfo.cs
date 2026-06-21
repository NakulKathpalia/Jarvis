namespace Jarvis.Auth;

public sealed record AuthProviderInfo(
    string Id,
    string Name,
    AuthProviderType Type,
    bool Configured,
    string Message);
