using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.Extensions;
using GhostNetwork.Account.Mongo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GhostNetwork.Account.Web.Pages.Settings.Secrets;

[Authorize]
public class Index : PageModel
{
    private readonly ClientsStorage clientsStorage;

    public Index(ClientsStorage clientsStorage)
    {
        this.clientsStorage = clientsStorage;
    }

    public IEnumerable<SecretModel> PasswordSecrets { get; set; }

    public async Task<ActionResult> OnGetAsync()
    {
        var clients = await clientsStorage.FindManyAsync(User.GetSubjectId());
        PasswordSecrets = clients
            .SelectMany(c => c.Secrets.Select(s => new SecretModel(c.Id, c.Name, DateTimeOffset.FromUnixTimeMilliseconds(s.Expiration).DateTime)))
            .ToList();
        return Page();
    }

    public record SecretModel(string Id, string Description, DateTime Expiration);
}
