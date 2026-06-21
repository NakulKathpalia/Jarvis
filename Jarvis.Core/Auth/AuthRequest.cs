namespace Jarvis.Auth;

public sealed record AuthRequest(string? Email, string? Password, string? Provider = null);
