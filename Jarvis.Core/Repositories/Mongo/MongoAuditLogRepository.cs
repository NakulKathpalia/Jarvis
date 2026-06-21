using Jarvis.Models;
using Jarvis.Mongo;
using MongoDB.Driver;

namespace Jarvis.Repositories.Mongo;

public sealed class MongoAuditLogRepository : IAuditLogRepository
{
    private readonly IMongoCollection<InteractionLogEntry> _logs;

    public MongoAuditLogRepository(MongoContext context)
    {
        _logs = context.Collection<InteractionLogEntry>(MongoCollectionNames.AuditLogs);
    }

    public async Task<IReadOnlyList<InteractionLogEntry>> GetRecentAsync(string userId, int limit, CancellationToken cancellationToken = default) =>
        await _logs.Find(log => log.UserId == userId)
            .SortByDescending(log => log.TimestampUtc)
            .Limit(limit)
            .ToListAsync(cancellationToken);

    public Task AddAsync(string userId, InteractionLogEntry entry, CancellationToken cancellationToken = default)
    {
        entry.Id = string.IsNullOrWhiteSpace(entry.Id) ? Guid.NewGuid().ToString("N") : entry.Id;
        entry.UserId = userId;
        entry.TimestampUtc = entry.TimestampUtc == default ? DateTime.UtcNow : entry.TimestampUtc;
        entry.UpdatedAtUtc = DateTime.UtcNow;
        return _logs.ReplaceOneAsync(
            log => log.Id == entry.Id,
            entry,
            new ReplaceOptions { IsUpsert = true },
            cancellationToken);
    }

    public Task ClearAsync(string userId, CancellationToken cancellationToken = default) =>
        _logs.DeleteManyAsync(log => log.UserId == userId, cancellationToken);
}
