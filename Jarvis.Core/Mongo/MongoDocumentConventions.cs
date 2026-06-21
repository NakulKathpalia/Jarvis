using MongoDB.Bson.Serialization.Conventions;

namespace Jarvis.Mongo;

public static class MongoDocumentConventions
{
    private static bool _registered;

    public static void Register()
    {
        if (_registered)
        {
            return;
        }

        var pack = new ConventionPack
        {
            new IgnoreExtraElementsConvention(true),
            new EnumRepresentationConvention(MongoDB.Bson.BsonType.String)
        };
        ConventionRegistry.Register("JarvisConventions", pack, _ => true);
        _registered = true;
    }
}
