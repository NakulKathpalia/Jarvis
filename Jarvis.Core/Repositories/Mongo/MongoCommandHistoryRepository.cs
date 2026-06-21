using Jarvis.Models;
using Jarvis.Mongo;
using MongoDB.Driver;

namespace Jarvis.Repositories.Mongo;

public sealed class MongoCommandHistoryRepository : ICommandHistoryRepository
{
    private readonly IMongoCollection<PcCommandLogEntry> _logs;

    public MongoCommandHistoryRepository(MongoContext context)
    {
        _logs = context.Collection<PcCommandLogEntry>(MongoCollectionNames.CommandHistory);
    }

    public async Task<IReadOnlyList<PcCommandLogEntry>> GetRecentAsync(string userId, int limit, CancellationToken cancellationToken = default) =>
        await _logs.Find(log => log.UserId == userId)
            .SortByDescending(log => log.TimestampUtc)
            .Limit(limit)
            .ToListAsync(cancellationToken);

    public Task AddAsync(string userId, PcCommandLogEntry entry, CancellationToken cancellationToken = default)
    {
        entry.Id = string.IsNullOrWhiteSpace(entry.Id) ? Guid.NewGuid().ToString("N") : entry.Id;
        entry.UserId = userId;
        entry.TimestampUtc = entry.TimestampUtc == default ? DateTime.UtcNow : entry.TimestampUtc;
        entry.UpdatedAtUtc = DateTime.UtcNow;
        return _logs.InsertOneAsync(entry, cancellationToken: cancellationToken);
    }
}
