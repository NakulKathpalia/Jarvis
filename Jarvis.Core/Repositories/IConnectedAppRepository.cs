using Jarvis.ConnectedApps;

namespace Jarvis.Repositories;

public interface IConnectedAppRepository
{
    Task<IReadOnlyList<ConnectedAppInfo>> GetForUserAsync(string userId, CancellationToken cancellationToken = default);
    Task UpsertAsync(string userId, ConnectedAppInfo app, CancellationToken cancellationToken = default);
}
