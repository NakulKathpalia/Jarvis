namespace Jarvis.Auth;

public interface IAuthService
{
    AuthStatus GetStatus();
    IReadOnlyCollection<AuthProviderInfo> GetProviders();
    AuthResponse SignIn(SignInRequest request);
    AuthResponse SignUp(SignUpRequest request);
    AuthResponse SignOut();
}
