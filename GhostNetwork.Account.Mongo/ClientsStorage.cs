using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace GhostNetwork.Account.Mongo;

public class ClientsStorage
{
    private readonly MongoDbContext context;

    public ClientsStorage(MongoDbContext context)
    {
        this.context = context;
    }

    public async Task<IEnumerable<ClientEntity>> FindManyAsync(string? user = null)
    {
        var filter = string.IsNullOrEmpty(user)
            ? Builders<ClientEntity>.Filter.Empty
            : Builders<ClientEntity>.Filter.Eq(c => c.Owner, user);

        return await context.Clients.Find(filter).ToListAsync();
    }

    public async Task<ClientEntity?> FindOneAsync(string clientId)
    {
        var filter = Builders<ClientEntity>.Filter.Eq(c => c.Id, clientId);

        return await context.Clients.Find(filter).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(ClientEntity client)
    {
        await context.Clients.InsertOneAsync(client);
    }

    public async Task DeleteOneAsync(string clientId)
    {
        var filter = Builders<ClientEntity>.Filter.Eq(c => c.Id, clientId);

        await context.Clients.DeleteOneAsync(filter);
    }
}