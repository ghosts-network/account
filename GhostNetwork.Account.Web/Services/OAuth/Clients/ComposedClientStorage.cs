using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;

namespace GhostNetwork.Account.Web.Services.OAuth.Clients;

public class ComposedClientStorage : IClientStore
{
    private readonly IClientStore[] storages;

    public ComposedClientStorage(params IClientStore[] storages)
    {
        this.storages = storages;
    }

    public async Task<Client> FindClientByIdAsync(string clientId)
    {
        foreach (var storage in storages)
        {
            var c = await storage.FindClientByIdAsync(clientId);
            if (c != null)
            {
                return c;
            }
        }

        return null;
    }
}