using IdentityServer4.Models;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Test;

namespace GhostNetwork.Account.Web
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources => new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile()
        };

        public static IEnumerable<ApiScope> ApiScopes => new[]
        {
            new ApiScope("api", "Full Access to API")
        };

        public static IEnumerable<ApiResource> ApiResources => new[]
        {
            new ApiResource("api", "Main API")
            {
                Scopes = {"api"}
            }
        };

        public static IEnumerable<Client> Clients => new[]
        {
            new Client
            {
                ClientId = "angular_spa",
                ClientName = "Angular Client",
                AllowedGrantTypes = GrantTypes.Implicit,
                AllowedScopes = new List<string> {"openid", "profile", "api"},
                RedirectUris = new List<string> {"http://localhost:4200/auth-callback"},
                PostLogoutRedirectUris = new List<string> {"http://localhost:4200/"},
                AllowedCorsOrigins = new List<string> {"http://localhost:4200"},
                AllowAccessTokensViaBrowser = true
            },
            new Client
            {
                ClientId = "angular_spa_prod",
                ClientName = "Angular Client",
                AllowedGrantTypes = GrantTypes.Implicit,
                AllowedScopes = new List<string> {"openid", "profile", "api"},
                RedirectUris = new List<string> {"https://gn.boberneprotiv.com/auth-callback"},
                PostLogoutRedirectUris = new List<string> {"https://gn.boberneprotiv.com/"},
                AllowedCorsOrigins = new List<string> {"https://gn.boberneprotiv.com"},
                AllowAccessTokensViaBrowser = true
            }
        };

        public static List<TestUser> Users => new List<TestUser>
        {
            new TestUser
            {
                SubjectId = "818727",
                Username = "alice",
                Password = "alice",
                Claims =
                {
                    new Claim(JwtClaimTypes.Name, "Alice Smith"),
                    new Claim(JwtClaimTypes.GivenName, "Alice"),
                    new Claim(JwtClaimTypes.FamilyName, "Smith"),
                    new Claim(JwtClaimTypes.Email, "AliceSmith@email.com"),
                    new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                    new Claim(JwtClaimTypes.WebSite, "http://alice.com"),
                    new Claim(JwtClaimTypes.Address, JsonSerializer.Serialize(new
                    {
                        street_address = "One Hacker Way",
                        locality = "Heidelberg",
                        postal_code = 69118,
                        country = "Germany"
                    }), IdentityServerConstants.ClaimValueTypes.Json)
                }
            },
            new TestUser
            {
                SubjectId = "88421113",
                Username = "bob",
                Password = "bob",
                Claims =
                {
                    new Claim(JwtClaimTypes.Name, "Bob Smith"),
                    new Claim(JwtClaimTypes.GivenName, "Bob"),
                    new Claim(JwtClaimTypes.FamilyName, "Smith"),
                    new Claim(JwtClaimTypes.Email, "BobSmith@email.com"),
                    new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                    new Claim(JwtClaimTypes.WebSite, "http://bob.com")
                }
            }
        };
    }
}
