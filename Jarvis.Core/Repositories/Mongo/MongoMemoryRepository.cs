using Jarvis.Models;
using Jarvis.Mongo;
using MongoDB.Driver;

namespace Jarvis.Repositories.Mongo;

public sealed class MongoMemoryRepository : IMemoryRepository
{
    private readonly IMongoCollection<MemoryItem> _memories;

    public MongoMemoryRepository(MongoContext context)
    {
        _memories = context.Collection<MemoryItem>(MongoCollectionNames.Memories);
    }

    public async Task<IReadOnlyList<MemoryItem>> GetForUserAsync(string userId, CancellationToken cancellationToken = default) =>
        await _memories.Find(memory => memory.UserId == userId)
            .SortByDescending(memory => memory.UpdatedAtUtc)
            .ToListAsync(cancellationToken);

    public Task UpsertAsync(string userId, MemoryItem item, CancellationToken cancellationToken = default)
    {
        item.UserId = userId;
        item.UpdatedAtUtc = DateTime.UtcNow;
        item.CreatedAtUtc = item.CreatedAtUtc == default ? item.UpdatedAtUtc : item.CreatedAtUtc;
        return _memories.ReplaceOneAsync(memory => memory.UserId == userId && memory.Id == item.Id, item, new ReplaceOptions { IsUpsert = true }, cancellationToken);
    }

    public Task DeleteAsync(string userId, string id, CancellationToken cancellationToken = default) =>
        _memories.DeleteOneAsync(memory => memory.UserId == userId && memory.Id == id, cancellationToken);

    public Task ClearAsync(string userId, CancellationToken cancellationToken = default) =>
        _memories.DeleteManyAsync(memory => memory.UserId == userId, cancellationToken);
}
