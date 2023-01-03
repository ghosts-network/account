using System;
using Duende.IdentityServer.Models;
using GhostNetwork.Account.Web.Quickstart.Account;
using Microsoft.AspNetCore.Mvc;

namespace GhostNetwork.Account.Web.Quickstart
{
    public static class Extensions
    {
        /// <summary>
        /// Checks if the redirect URI is for a native client.
        /// </summary>
        /// <param name="context">AuthorizationRequest context.</param>
        public static bool IsNativeClient(this AuthorizationRequest context)
        {
            return !context.RedirectUri.StartsWith("https", StringComparison.Ordinal)
               && !context.RedirectUri.StartsWith("http", StringComparison.Ordinal);
        }

        public static IActionResult LoadingPage(this Controller controller, string viewName, string redirectUri)
        {
            controller.HttpContext.Response.StatusCode = 200;
            controller.HttpContext.Response.Headers["Location"] = string.Empty;

            return controller.View(viewName, new RedirectViewModel { RedirectUrl = redirectUri });
        }
    }
}
