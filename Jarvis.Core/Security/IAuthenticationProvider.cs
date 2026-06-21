namespace Jarvis.Security;

public interface IAuthenticationProvider
{
    Task<AuthenticationResult> AuthenticateAsync(string scheme, string credential, CancellationToken cancellationToken = default);
}

public sealed record AuthenticationResult(bool Succeeded, string PrincipalId, string Reason);
