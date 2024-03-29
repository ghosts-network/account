using MongoDB.Bson.Serialization.Attributes;

namespace GhostNetwork.Account.Mongo;

public class ClientSecretEntity
{
    [BsonElement("value")]
    public string Value { get; set; } = null!;

    [BsonElement("expiration")]
    public long Expiration { get; set; }
}