namespace Jarvis.Security;

public interface IAuthorizationService
{
    Task<bool> AuthorizeAsync(string principalId, string resource, string action, CancellationToken cancellationToken = default);
}
