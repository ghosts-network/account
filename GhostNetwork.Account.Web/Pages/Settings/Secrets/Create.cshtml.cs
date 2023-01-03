using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using GhostNetwork.Account.Mongo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GhostNetwork.Account.Web.Pages.Settings.Secrets;

[Authorize]
public class Create : PageModel
{
    private readonly ClientsStorage clientsStorage;

    public Create(ClientsStorage clientsStorage)
    {
        this.clientsStorage = clientsStorage;
        Secret = new CreateSecretModel(string.Empty);
    }

    [BindProperty]
    public CreateSecretModel Secret { get; set; }

    public ResultsModel Result { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        var clientId = Guid.NewGuid().ToString();
        var secret = Guid.NewGuid().ToString();
        var expiration = DateTimeOffset.UtcNow.AddMonths(3);
        await clientsStorage.CreateAsync(new ClientEntity
        {
            Id = clientId,
            Name = Secret.Description,
            GrandType = GrantType.ResourceOwnerPassword,
            Secrets = new List<ClientSecretEntity>
            {
                new ClientSecretEntity
                {
                    Value = secret.Sha256(),
                    Expiration = expiration.ToUnixTimeMilliseconds()
                }
            },
            Owner = User.GetSubjectId()
        });

        Result = new ResultsModel(Secret.Description, expiration.DateTime, clientId, secret);
        return Page();
    }
}

public record CreateSecretModel(string Description);

public record ResultsModel(string Description, DateTime Expiration, string ClientId, string PlainSecret);
