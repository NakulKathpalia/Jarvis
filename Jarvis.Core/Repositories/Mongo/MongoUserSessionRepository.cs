using Jarvis.Mongo;
using Jarvis.Users;
using MongoDB.Driver;

namespace Jarvis.Repositories.Mongo;

public sealed class MongoUserSessionRepository : IUserSessionRepository
{
    private readonly IMongoCollection<UserSession> _sessions;

    public MongoUserSessionRepository(MongoContext context)
    {
        _sessions = context.Collection<UserSession>(MongoCollectionNames.UserSessions);
    }

    public Task AddAsync(UserSession session, CancellationToken cancellationToken = default) =>
        _sessions.InsertOneAsync(session, cancellationToken: cancellationToken);

    public async Task<IReadOnlyList<UserSession>> GetActiveForUserAsync(string userId, CancellationToken cancellationToken = default) =>
        await _sessions.Find(session => session.UserId == userId && session.RevokedAtUtc == null && session.ExpiresAtUtc > DateTime.UtcNow)
            .SortByDescending(session => session.LastSeenAtUtc)
            .ToListAsync(cancellationToken);

    public Task RevokeAsync(string sessionId, CancellationToken cancellationToken = default) =>
        _sessions.UpdateOneAsync(
            session => session.Id == sessionId,
            Builders<UserSession>.Update
                .Set(session => session.RevokedAtUtc, DateTime.UtcNow)
                .Set(session => session.UpdatedAtUtc, DateTime.UtcNow),
            cancellationToken: cancellationToken);
}
