using Jarvis.Users;

namespace Jarvis.Repositories;

public interface IUserRepository
{
    Task<UserAccount?> GetAsync(string userId, CancellationToken cancellationToken = default);
    Task UpsertAsync(UserAccount user, CancellationToken cancellationToken = default);
}
