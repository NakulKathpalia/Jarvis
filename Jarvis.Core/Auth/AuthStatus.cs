namespace Jarvis.Auth;

public sealed record AuthStatus(bool IsAuthenticated, AuthUser? User, string Message);
