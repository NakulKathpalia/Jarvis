using Jarvis.Models;
using Jarvis.Mongo;
using MongoDB.Driver;

namespace Jarvis.Repositories.Mongo;

public sealed class MongoChatRepository : IChatRepository
{
    private readonly IMongoCollection<ChatSession> _sessions;
    private readonly IMongoCollection<ChatMessage> _messages;

    public MongoChatRepository(MongoContext context)
    {
        _sessions = context.Collection<ChatSession>(MongoCollectionNames.ChatSessions);
        _messages = context.Collection<ChatMessage>(MongoCollectionNames.ChatMessages);
    }

    public async Task<IReadOnlyList<ChatSession>> GetSessionsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var sessions = await _sessions.Find(session => session.UserId == userId)
            .SortByDescending(session => session.UpdatedAtUtc)
            .ToListAsync(cancellationToken);

        foreach (var session in sessions)
        {
            session.Messages = await GetMessagesAsync(userId, session.Id, cancellationToken);
        }

        return sessions;
    }

    public async Task<ChatSession?> GetSessionAsync(string userId, string id, CancellationToken cancellationToken = default)
    {
        var session = await _sessions.Find(item => item.UserId == userId && item.Id == id).FirstOrDefaultAsync(cancellationToken);
        if (session is not null)
        {
            session.Messages = await GetMessagesAsync(userId, session.Id, cancellationToken);
        }

        return session;
    }

    public Task UpsertSessionAsync(string userId, ChatSession session, CancellationToken cancellationToken = default)
    {
        session.UserId = userId;
        session.UpdatedAtUtc = session.UpdatedAtUtc == default ? DateTime.UtcNow : session.UpdatedAtUtc;
        session.CreatedAtUtc = session.CreatedAtUtc == default ? session.UpdatedAtUtc : session.CreatedAtUtc;
        var storedSession = new ChatSession
        {
            Id = session.Id,
            UserId = session.UserId,
            Title = session.Title,
            CreatedAtUtc = session.CreatedAtUtc,
            UpdatedAtUtc = session.UpdatedAtUtc,
            Messages = []
        };
        return _sessions.ReplaceOneAsync(item => item.UserId == userId && item.Id == session.Id, storedSession, new ReplaceOptions { IsUpsert = true }, cancellationToken);
    }

    public async Task AddMessageAsync(string userId, string sessionId, ChatMessage message, CancellationToken cancellationToken = default)
    {
        message.Id = string.IsNullOrWhiteSpace(message.Id) ? Guid.NewGuid().ToString("N") : message.Id;
        message.UserId = userId;
        message.ChatSessionId = sessionId;
        message.CreatedAtUtc = message.CreatedAtUtc == default ? DateTime.UtcNow : message.CreatedAtUtc;
        message.UpdatedAtUtc = DateTime.UtcNow;
        await _messages.InsertOneAsync(message, cancellationToken: cancellationToken);
    }

    public async Task DeleteSessionAsync(string userId, string id, CancellationToken cancellationToken = default)
    {
        await _sessions.DeleteOneAsync(session => session.UserId == userId && session.Id == id, cancellationToken);
        await _messages.DeleteManyAsync(message => message.UserId == userId && message.ChatSessionId == id, cancellationToken);
    }

    private async Task<List<ChatMessage>> GetMessagesAsync(string userId, string sessionId, CancellationToken cancellationToken) =>
        await _messages.Find(message => message.UserId == userId && message.ChatSessionId == sessionId)
            .SortBy(message => message.CreatedAtUtc)
            .ToListAsync(cancellationToken);
}
