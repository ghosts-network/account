using System.Linq;
using System.Threading.Tasks;

namespace GhostNetwork.Account.Web
{
    public interface IDefaultClientProvider
    {
        Task<string> GetUrlAsync();
    }

    public class DefaultClientProvider : IDefaultClientProvider
    {
        private readonly string clientKey;

        public DefaultClientProvider(string clientKey)
        {
            this.clientKey = clientKey;
        }

        public Task<string> GetUrlAsync()
        {
            var client = Config.Clients.First(c => c.ClientId == clientKey);
            return Task.FromResult(client.ClientUri);
        }
    }
}