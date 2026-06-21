namespace Jarvis.Auth;

public sealed record SignUpRequest(string? Email, string? Password, string? Name = null);
