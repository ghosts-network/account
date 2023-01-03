using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GhostNetwork.Account.Mongo;

public class ClientEntity
{
    [BsonId]
    public string Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; }

    [BsonElement("grantType")]
    public string GrandType { get; set; }

    [BsonElement("secrets")]
    public List<ClientSecretEntity> Secrets { get; set; }

    [BsonElement("owner")]
    public string Owner { get; set; }
}