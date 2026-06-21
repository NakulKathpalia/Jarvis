namespace Jarvis.Auth;

public interface IAuthService
{
    AuthStatus GetStatus();
    IReadOnlyCollection<AuthProviderInfo> GetProviders();
    AuthResponse SignIn(AuthRequest request);
    AuthResponse SignUp(AuthRequest request);
    AuthResponse SignOut();
}
