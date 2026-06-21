using Jarvis.Models;
using Jarvis.Users;
using MongoDB.Driver;

namespace Jarvis.Mongo;

public sealed class MongoInitializer
{
    private readonly MongoContext _context;

    public MongoInitializer(MongoContext context)
    {
        _context = context;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);
        await EnsureDefaultOwnerAsync(cancellationToken);
    }

    private async Task EnsureIndexesAsync(CancellationToken cancellationToken)
    {
        await _context.Collection<UserAccount>(MongoCollectionNames.Users).Indexes.CreateOneAsync(
            new CreateIndexModel<UserAccount>(
                Builders<UserAccount>.IndexKeys.Ascending(user => user.Email),
                new CreateIndexOptions { Unique = true }),
            cancellationToken: cancellationToken);

        await _context.Collection<MemoryItem>(MongoCollectionNames.Memories).Indexes.CreateManyAsync(
            [
                new CreateIndexModel<MemoryItem>(Builders<MemoryItem>.IndexKeys.Ascending("UserId").Descending(item => item.UpdatedAtUtc)),
                new CreateIndexModel<MemoryItem>(Builders<MemoryItem>.IndexKeys.Text(item => item.Text).Text(item => item.Category))
            ],
            cancellationToken);

        await _context.Collection<ChatSession>(MongoCollectionNames.ChatSessions).Indexes.CreateOneAsync(
            new CreateIndexModel<ChatSession>(Builders<ChatSession>.IndexKeys.Ascending("UserId").Descending(session => session.UpdatedAtUtc)),
            cancellationToken: cancellationToken);

        await _context.Collection<ChatMessage>(MongoCollectionNames.ChatMessages).Indexes.CreateOneAsync(
            new CreateIndexModel<ChatMessage>(Builders<ChatMessage>.IndexKeys.Ascending("UserId").Ascending("ChatSessionId").Ascending(message => message.CreatedAtUtc)),
            cancellationToken: cancellationToken);

        await _context.Collection<InteractionLogEntry>(MongoCollectionNames.AuditLogs).Indexes.CreateOneAsync(
            new CreateIndexModel<InteractionLogEntry>(Builders<InteractionLogEntry>.IndexKeys.Ascending("UserId").Descending(log => log.TimestampUtc)),
            cancellationToken: cancellationToken);

        await _context.Collection<PcCommandLogEntry>(MongoCollectionNames.CommandHistory).Indexes.CreateOneAsync(
            new CreateIndexModel<PcCommandLogEntry>(Builders<PcCommandLogEntry>.IndexKeys.Ascending("UserId").Descending(log => log.TimestampUtc)),
            cancellationToken: cancellationToken);
    }

    private async Task EnsureDefaultOwnerAsync(CancellationToken cancellationToken)
    {
        var users = _context.Collection<UserAccount>(MongoCollectionNames.Users);
        var existing = await users.Find(user => user.Id == JarvisUserContext.DefaultOwnerUserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing is not null)
        {
            return;
        }

        var now = DateTime.UtcNow;
        await users.InsertOneAsync(new UserAccount
        {
            Id = JarvisUserContext.DefaultOwnerUserId,
            Email = "local-owner@jarvis.local",
            DisplayName = "Local Owner",
            Roles = [UserRole.Owner],
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        }, cancellationToken: cancellationToken);
    }
}
