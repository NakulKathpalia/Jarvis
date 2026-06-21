using Jarvis.ConnectedApps;
using Jarvis.Mongo;
using MongoDB.Driver;

namespace Jarvis.Repositories.Mongo;

public sealed class MongoConnectedAppRepository : IConnectedAppRepository
{
    private readonly IMongoCollection<ConnectedAppInfo> _apps;

    public MongoConnectedAppRepository(MongoContext context)
    {
        _apps = context.Collection<ConnectedAppInfo>(MongoCollectionNames.ConnectedApps);
    }

    public async Task<IReadOnlyList<ConnectedAppInfo>> GetForUserAsync(string userId, CancellationToken cancellationToken = default) =>
        await _apps.Find(app => app.UserId == userId)
            .SortBy(app => app.Name)
            .ToListAsync(cancellationToken);

    public Task UpsertAsync(string userId, ConnectedAppInfo app, CancellationToken cancellationToken = default)
    {
        app.UserId = userId;
        app.UpdatedAtUtc = DateTime.UtcNow;
        app.CreatedAtUtc = app.CreatedAtUtc == default ? app.UpdatedAtUtc : app.CreatedAtUtc;
        return _apps.ReplaceOneAsync(item => item.UserId == userId && item.Id == app.Id, app, new ReplaceOptions { IsUpsert = true }, cancellationToken);
    }
}
