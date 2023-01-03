using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using GhostNetwork.Account.Mongo;

namespace GhostNetwork.Account.Web.Services.OAuth.Clients;

public class MongoClientStorage : IClientStore
{
    private readonly ClientsStorage clientsStorage;

    public MongoClientStorage(ClientsStorage clientsStorage)
    {
        this.clientsStorage = clientsStorage;
    }

    public async Task<Client> FindClientByIdAsync(string clientId)
    {
        var client = await clientsStorage.FindOneAsync(clientId);

        if (client == null)
        {
            return null;
        }

        return new Client
        {
            ClientId = client.Id.ToString(),
            ClientName = client.Name,
            AllowedGrantTypes = new List<string> { client.GrandType },
            AllowedScopes = new List<string> { "openid", "profile", "api" },
            ClientSecrets = client.Secrets
                .Select(s => new Secret(
                    s.Value,
                    DateTimeOffset.FromUnixTimeMilliseconds(s.Expiration).DateTime))
                .ToList()
        };
    }
}