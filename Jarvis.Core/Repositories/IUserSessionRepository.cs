using Jarvis.Users;

namespace Jarvis.Repositories;

public interface IUserSessionRepository
{
    Task AddAsync(UserSession session, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserSession>> GetActiveForUserAsync(string userId, CancellationToken cancellationToken = default);
    Task RevokeAsync(string sessionId, CancellationToken cancellationToken = default);
}
