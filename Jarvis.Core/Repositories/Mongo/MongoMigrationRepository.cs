using Jarvis.Migrations;
using Jarvis.Mongo;
using MongoDB.Driver;

namespace Jarvis.Repositories.Mongo;

public sealed class MongoMigrationRepository : IMigrationRepository
{
    private readonly IMongoCollection<MigrationRecord> _migrations;

    public MongoMigrationRepository(MongoContext context)
    {
        _migrations = context.Collection<MigrationRecord>(MongoCollectionNames.Migrations);
    }

    public async Task<bool> HasCompletedAsync(string name, string version, CancellationToken cancellationToken = default)
    {
        var record = await _migrations.Find(migration =>
                migration.Name == name
                && migration.Version == version
                && migration.Status == "Completed")
            .FirstOrDefaultAsync(cancellationToken);
        return record is not null;
    }

    public Task CompleteAsync(string name, string version, string message, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var record = new MigrationRecord
        {
            Name = name,
            Version = version,
            Status = "Completed",
            StartedAtUtc = now,
            CompletedAtUtc = now,
            Message = message
        };
        return _migrations.ReplaceOneAsync(
            migration => migration.Name == name && migration.Version == version,
            record,
            new ReplaceOptions { IsUpsert = true },
            cancellationToken);
    }
}
