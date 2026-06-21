namespace Jarvis.Mongo;

public sealed class MongoOptions
{
    public string ConnectionString { get; init; } = "mongodb://localhost:27017";
    public string DatabaseName { get; init; } = "jarvis_local";
    public TimeSpan ServerSelectionTimeout { get; init; } = TimeSpan.FromMilliseconds(800);
}
