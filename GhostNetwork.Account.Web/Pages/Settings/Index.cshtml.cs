using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GhostNetwork.Account.Web.Pages.Settings;

[Authorize]
public class Index : PageModel
{
    public void OnGet()
    {
        
    }
}