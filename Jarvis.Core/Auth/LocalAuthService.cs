namespace Jarvis.Auth;

public sealed class LocalAuthService : IAuthService
{
    private readonly AuthService _inner = new();

    public AuthStatus GetStatus() => _inner.GetStatus();

    public IReadOnlyCollection<AuthProviderInfo> GetProviders() => _inner.GetProviders();

    public AuthResponse SignIn(SignInRequest request) => _inner.SignIn(request);

    public AuthResponse SignUp(SignUpRequest request) => _inner.SignUp(request);

    public AuthResponse SignOut() => _inner.SignOut();
}
