namespace Jarvis.Auth;

public sealed record AuthStatus(bool IsAuthenticated, string? UserName, string Message);
