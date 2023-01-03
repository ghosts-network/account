using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GhostNetwork.Account.Mongo;

public class ClientEntity
{
    [BsonId]
    public string Id { get; set; } = null!;

    [BsonElement("name")]
    public string Name { get; set; } = null!;

    [BsonElement("grantType")]
    public string GrandType { get; set; } = null!;

    [BsonElement("secrets")]
    public List<ClientSecretEntity> Secrets { get; set; } = null!;

    [BsonElement("owner")]
    public string Owner { get; set; } = null!;
}