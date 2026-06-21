namespace Jarvis.Security;

public interface IRateLimiter
{
    Task<bool> IsAllowedAsync(string bucket, string key, CancellationToken cancellationToken = default);
}
