using Jarvis.Models;
using Jarvis.Mongo;
using MongoDB.Driver;

namespace Jarvis.Repositories.Mongo;

public sealed class MongoVoiceHistoryRepository : IVoiceHistoryRepository
{
    private readonly IMongoCollection<VoiceHistoryItem> _items;

    public MongoVoiceHistoryRepository(MongoContext context)
    {
        _items = context.Collection<VoiceHistoryItem>(MongoCollectionNames.VoiceHistory);
    }

    public async Task<IReadOnlyList<VoiceHistoryItem>> GetRecentAsync(string userId, int limit, CancellationToken cancellationToken = default) =>
        await _items.Find(item => item.UserId == userId)
            .SortByDescending(item => item.TimestampUtc)
            .Limit(limit)
            .ToListAsync(cancellationToken);

    public Task AddAsync(string userId, VoiceHistoryItem item, CancellationToken cancellationToken = default)
    {
        item.Id = string.IsNullOrWhiteSpace(item.Id) ? Guid.NewGuid().ToString("N") : item.Id;
        item.UserId = userId;
        item.TimestampUtc = item.TimestampUtc == default ? DateTime.UtcNow : item.TimestampUtc;
        item.UpdatedAtUtc = DateTime.UtcNow;
        return _items.InsertOneAsync(item, cancellationToken: cancellationToken);
    }
}
