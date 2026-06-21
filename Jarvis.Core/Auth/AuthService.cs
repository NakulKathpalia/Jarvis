using System.Security.Cryptography;

namespace Jarvis.Auth;

public sealed class AuthService : IAuthService
{
    private readonly object _lock = new();
    private AuthUser? _currentUser;

    public AuthStatus GetStatus()
    {
        lock (_lock)
        {
            return _currentUser is null
                ? new(false, null, "Signed out. Local placeholder authentication is available for UI testing.")
                : new(true, _currentUser, $"Signed in as {_currentUser.Email}.");
        }
    }

    public IReadOnlyCollection<AuthProviderInfo> GetProviders() =>
    [
        new("local", "Email and password", AuthProviderType.Local, true, "Local placeholder auth is enabled for development only."),
        new("google", "Google", AuthProviderType.Google, false, "OAuth is not configured yet."),
        new("microsoft", "Microsoft", AuthProviderType.Microsoft, false, "OAuth is not configured yet."),
        new("github", "GitHub", AuthProviderType.GitHub, false, "OAuth is not configured yet."),
        new("discord", "Discord", AuthProviderType.Discord, false, "OAuth is not configured yet.")
    ];

    public AuthResponse SignIn(SignInRequest request)
    {
        var validation = ValidateLocalCredentials(request.Email, request.Password);
        if (validation is not null)
        {
            return new(false, validation, GetStatus());
        }

        var user = CreatePlaceholderUser(request.Email!, request.Email!);
        lock (_lock)
        {
            _currentUser = user;
        }

        return new(true, "Signed in with local placeholder auth. No token was created.", GetStatus());
    }

    public AuthResponse SignUp(SignUpRequest request)
    {
        var validation = ValidateLocalCredentials(request.Email, request.Password);
        if (validation is not null)
        {
            return new(false, validation, GetStatus());
        }

        var displayName = string.IsNullOrWhiteSpace(request.Name) ? request.Email! : request.Name.Trim();
        var user = CreatePlaceholderUser(request.Email!, displayName);
        lock (_lock)
        {
            _currentUser = user;
        }

        return new(true, "Placeholder account created for this process only. No password or token was stored.", GetStatus());
    }

    public AuthResponse SignOut()
    {
        lock (_lock)
        {
            _currentUser = null;
        }

        return new(true, "Signed out of the local placeholder session.", GetStatus());
    }

    private static string? ValidateLocalCredentials(string? email, string? password)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return "Email is required.";
        }

        if (!email.Contains('@', StringComparison.Ordinal))
        {
            return "Enter a valid email address.";
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return "Password is required.";
        }

        return null;
    }

    private static AuthUser CreatePlaceholderUser(string email, string name)
    {
        var normalizedEmail = email.Trim();
        var hash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(normalizedEmail.ToUpperInvariant()));
        var userId = Convert.ToHexString(hash)[..16];
        return new AuthUser(userId, normalizedEmail, name.Trim(), AuthProviderType.Local);
    }
}
