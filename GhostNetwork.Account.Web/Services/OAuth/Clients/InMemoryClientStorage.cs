using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;

namespace GhostNetwork.Account.Web.Services.OAuth.Clients;

public class InMemoryClientStorage : IClientStore
{
    private readonly IEnumerable<Client> clients;

    public InMemoryClientStorage(IEnumerable<Client> clients)
    {
        this.clients = clients;
    }

    public Task<Client> FindClientByIdAsync(string clientId)
    {
        return Task.FromResult(clients.First(c => c.ClientId == clientId));
    }
}
