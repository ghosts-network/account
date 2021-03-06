using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;

namespace GhostNetwork.Account.Web
{
    public interface IDefaultClientProvider
    {
        Task<Client> GetDefaultClientAsync();
    }

    public class DefaultClientProvider : IDefaultClientProvider
    {
        private readonly string clientKey;

        public DefaultClientProvider(string clientKey)
        {
            this.clientKey = clientKey;
        }

        public Task<Client> GetDefaultClientAsync()
        {
            var client = Config.Clients.SingleOrDefault(c => c.ClientId == clientKey);
            return Task.FromResult(client);
        }
    }
}