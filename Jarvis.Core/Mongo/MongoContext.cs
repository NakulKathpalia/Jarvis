using MongoDB.Bson;
using MongoDB.Driver;

namespace Jarvis.Mongo;

public sealed class MongoContext
{
    public MongoContext(MongoOptions options)
    {
        var settings = MongoClientSettings.FromConnectionString(options.ConnectionString);
        settings.ServerSelectionTimeout = options.ServerSelectionTimeout;
        Client = new MongoClient(settings);
        Database = Client.GetDatabase(options.DatabaseName);
    }

    public IMongoClient Client { get; }
    public IMongoDatabase Database { get; }

    public IMongoCollection<T> Collection<T>(string name) => Database.GetCollection<T>(name);

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await Database.RunCommandAsync((Command<BsonDocument>)"{ping:1}", cancellationToken: cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
