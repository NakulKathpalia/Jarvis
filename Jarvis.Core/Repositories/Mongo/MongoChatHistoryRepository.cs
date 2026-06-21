using Jarvis.Models;
using Jarvis.Mongo;
using MongoDB.Driver;

namespace Jarvis.Repositories.Mongo;

public sealed class MongoChatHistoryRepository : IChatHistoryRepository
{
    private const string LegacyHistorySessionId = "legacy-history";
    private readonly IMongoCollection<ChatMessage> _messages;

    public MongoChatHistoryRepository(MongoContext context)
    {
        _messages = context.Collection<ChatMessage>(MongoCollectionNames.ChatMessages);
    }

    public async Task<IReadOnlyList<ChatMessage>> GetAsync(string userId, CancellationToken cancellationToken = default) =>
        await _messages.Find(message => message.UserId == userId && message.ChatSessionId == LegacyHistorySessionId)
            .SortBy(message => message.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public Task AddAsync(string userId, ChatMessage message, CancellationToken cancellationToken = default)
    {
        message.Id = string.IsNullOrWhiteSpace(message.Id) ? Guid.NewGuid().ToString("N") : message.Id;
        message.UserId = userId;
        message.ChatSessionId = LegacyHistorySessionId;
        message.CreatedAtUtc = message.CreatedAtUtc == default ? DateTime.UtcNow : message.CreatedAtUtc;
        message.UpdatedAtUtc = DateTime.UtcNow;
        return _messages.InsertOneAsync(message, cancellationToken: cancellationToken);
    }
}
