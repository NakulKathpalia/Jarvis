namespace Jarvis.Auth;

public sealed class AuthService : IAuthService
{
    private static readonly AuthStatus SignedOut = new(false, null, "Authentication is not configured yet.");

    public AuthStatus GetStatus() => SignedOut;

    public IReadOnlyCollection<AuthProviderInfo> GetProviders() =>
    [
        new("google", "Google", false, "OAuth is not configured."),
        new("microsoft", "Microsoft", false, "OAuth is not configured."),
        new("github", "GitHub", false, "OAuth is not configured."),
        new("discord", "Discord", false, "OAuth is not configured.")
    ];

    public AuthResponse SignIn(AuthRequest request) =>
        new(false, "Sign in is a placeholder. Configure an authentication provider before enabling accounts.", SignedOut);

    public AuthResponse SignUp(AuthRequest request) =>
        new(false, "Sign up is a placeholder. No account or token was created.", SignedOut);

    public AuthResponse SignOut() =>
        new(true, "Signed out locally. No server session existed.", SignedOut);
}
