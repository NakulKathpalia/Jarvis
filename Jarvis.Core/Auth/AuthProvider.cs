namespace Jarvis.Auth;

public sealed record AuthProvider(
    string Id,
    string Name,
    AuthProviderType Type,
    bool Configured,
    string Message);
