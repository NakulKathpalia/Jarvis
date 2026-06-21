namespace Jarvis.Auth;

public sealed record AuthResponse(bool Succeeded, string Message, AuthStatus Status);
