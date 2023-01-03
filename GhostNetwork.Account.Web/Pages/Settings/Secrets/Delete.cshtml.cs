using System;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.Extensions;
using GhostNetwork.Account.Mongo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GhostNetwork.Account.Web.Pages.Settings.Secrets;

[Authorize]
public class Delete : PageModel
{
    private readonly ClientsStorage clientsStorage;

    public SecretModel Secret { get; set; }

    public Delete(ClientsStorage clientsStorage)
    {
        this.clientsStorage = clientsStorage;
    }

    public async Task<ActionResult> OnGet([FromRoute] string clientId)
    {
        var client = await clientsStorage.FindOneAsync(clientId);
        if (client is null)
        {
            throw new Exception();
        }

        if (client.Owner != User.GetSubjectId())
        {
            throw new Exception();
        }

        Secret = new SecretModel(client.Id, client.Name, DateTimeOffset.FromUnixTimeMilliseconds(client.Secrets.First().Expiration).DateTime);

        return Page();
    }

    public async Task<ActionResult> OnPostAsync([FromRoute] string clientId)
    {
        await clientsStorage.DeleteOneAsync(clientId);
        return Redirect("/settings/secrets");
    }

    public record SecretModel(string Id, string Description, DateTime Expiration);
}
