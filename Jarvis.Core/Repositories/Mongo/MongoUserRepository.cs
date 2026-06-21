using Jarvis.Mongo;
using Jarvis.Users;
using MongoDB.Driver;

namespace Jarvis.Repositories.Mongo;

public sealed class MongoUserRepository : IUserRepository
{
    private readonly IMongoCollection<UserAccount> _users;

    public MongoUserRepository(MongoContext context)
    {
        _users = context.Collection<UserAccount>(MongoCollectionNames.Users);
    }

    public async Task<UserAccount?> GetAsync(string userId, CancellationToken cancellationToken = default) =>
        await _users.Find(user => user.Id == userId).FirstOrDefaultAsync(cancellationToken);

    public Task UpsertAsync(UserAccount user, CancellationToken cancellationToken = default)
    {
        user.UpdatedAtUtc = DateTime.UtcNow;
        return _users.ReplaceOneAsync(existing => existing.Id == user.Id, user, new ReplaceOptions { IsUpsert = true }, cancellationToken);
    }
}
