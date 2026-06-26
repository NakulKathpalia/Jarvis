using Jarvis.Models;
using Jarvis.Mongo;
using MongoDB.Driver;

namespace Jarvis.Repositories.Mongo;

public sealed class MongoKnowledgeRepository : IKnowledgeRepository
{
    private readonly IMongoCollection<KnowledgeItem> _knowledge;

    public MongoKnowledgeRepository(MongoContext context)
    {
        _knowledge = context.Collection<KnowledgeItem>(MongoCollectionNames.Knowledge);
    }

    public async Task<IReadOnlyList<KnowledgeItem>> GetForUserAsync(string userId, CancellationToken cancellationToken = default) =>
        await _knowledge.Find(item => item.UserId == userId)
            .SortByDescending(item => item.UpdatedAtUtc)
            .ToListAsync(cancellationToken);

    public Task UpsertAsync(string userId, KnowledgeItem item, CancellationToken cancellationToken = default)
    {
        item.UserId = userId;
        item.UpdatedAtUtc = DateTime.UtcNow;
        item.CreatedAtUtc = item.CreatedAtUtc == default ? item.UpdatedAtUtc : item.CreatedAtUtc;
        return _knowledge.ReplaceOneAsync(existing => existing.UserId == userId && existing.Id == item.Id, item, new ReplaceOptions { IsUpsert = true }, cancellationToken);
    }

    public Task DeleteAsync(string userId, string id, CancellationToken cancellationToken = default) =>
        _knowledge.DeleteOneAsync(item => item.UserId == userId && item.Id == id, cancellationToken);
}
