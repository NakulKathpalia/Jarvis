using Jarvis.Models;
using Jarvis.Mongo;
using MongoDB.Driver;

namespace Jarvis.Repositories.Mongo;

public sealed class MongoSettingsRepository : ISettingsRepository
{
    private readonly IMongoCollection<UserSettingDocument> _settings;

    public MongoSettingsRepository(MongoContext context)
    {
        _settings = context.Collection<UserSettingDocument>(MongoCollectionNames.Settings);
    }

    public async Task<AppSettings?> GetAsync(string userId, string scope, CancellationToken cancellationToken = default)
    {
        var document = await _settings.Find(setting => setting.UserId == userId && setting.Scope == scope)
            .FirstOrDefaultAsync(cancellationToken);
        return document?.Settings;
    }

    public Task UpsertAsync(string userId, string scope, AppSettings settings, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var update = Builders<UserSettingDocument>.Update
            .Set(document => document.Settings, settings)
            .Set(document => document.UpdatedAtUtc, now)
            .SetOnInsert(document => document.Id, Guid.NewGuid().ToString("N"))
            .SetOnInsert(document => document.UserId, userId)
            .SetOnInsert(document => document.Scope, scope)
            .SetOnInsert(document => document.CreatedAtUtc, now);

        return _settings.UpdateOneAsync(
            document => document.UserId == userId && document.Scope == scope,
            update,
            new UpdateOptions { IsUpsert = true },
            cancellationToken);
    }
}
