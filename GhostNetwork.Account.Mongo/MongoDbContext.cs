using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace GhostNetwork.Account.Mongo;

public class MongoDbContext
{
    private const string ProfilesCollection = "clients";

    private readonly IMongoDatabase database;

    static MongoDbContext()
    {
        BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
    }

    public MongoDbContext(IMongoDatabase database)
    {
        this.database = database;
    }

    public IMongoCollection<ClientEntity> Clients => database
        .GetCollection<ClientEntity>(ProfilesCollection);
}
