using System.Collections.Generic;
using System.Security.Claims;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Test;
using IdentityModel;

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
                ClientId = "ios_app",
                ClientName = "IOS Client",
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                AllowedScopes = new List<string> {"openid", "profile", "api"},
                ClientSecrets = new List<Secret>
                {
                    new Secret("temp_secret".Sha256())
                }
            },
            new Client
            {
                ClientId = "autotests_client",
                ClientName = "Autotests Client",
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                AllowedScopes = new List<string> {"openid", "profile", "api"},
                ClientSecrets = new List<Secret>
                {
                    new Secret("test_temp_secret".Sha256())
                }
            },
            new Client
            {
                ClientId = "angular_spa",
                ClientName = "Angular Client",
                AllowedGrantTypes = GrantTypes.Implicit,
                AllowedScopes = new List<string> { "openid", "profile", "api" },
                RedirectUris = new List<string> { "http://localhost:4200/auth-callback" },
                PostLogoutRedirectUris = new List<string> { "http://localhost:4200/" },
                ClientUri = "http://localhost:4200",
                AllowedCorsOrigins = new List<string> { "http://localhost:4200" },
                AllowAccessTokensViaBrowser = true
            },
            new Client
            {
                ClientId = "swagger_local",
                ClientName = "Swagger Local",
                ClientSecrets = { new Secret("secret".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,

                RedirectUris = new List<string> {"http://localhost:5000/swagger/oauth2-redirect.html"},
                AllowedCorsOrigins = new List<string> {"http://localhost:5000"},
                AllowedScopes = new List<string> {"openid", "profile", "api"}
            },
            new Client
            {
                ClientId = "swagger_prod",
                ClientName = "Swagger Prod",
                ClientSecrets = { new Secret("secret".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,

                RedirectUris = new List<string>
                {
                    "https://api.ghost-network.boberneprotiv.com/swagger/oauth2-redirect.html"
                },
                AllowedCorsOrigins = new List<string>
                {
                    "https://api.ghost-network.boberneprotiv.com"
                },
                AllowedScopes = new List<string> {"openid", "profile", "api"}
            },
            new Client
            {
                ClientId = "angular_spa_prod",
                ClientName = "Angular Client",
                AllowedGrantTypes = GrantTypes.Implicit,
                AllowedScopes = new List<string> {"openid", "profile", "api"},
                RedirectUris = new List<string>
                {
                    "https://ghost-network.boberneprotiv.com/auth-callback"
                },
                PostLogoutRedirectUris = new List<string>
                {
                    "https://ghost-network.boberneprotiv.com/"
                },
                AllowedCorsOrigins = new List<string>
                {
                    "https://ghost-network.boberneprotiv.com"
                },
                AllowAccessTokensViaBrowser = true
            },
            new Client
            {
                ClientId = "cockpit_local",
                ClientName = "Cockpit Client",
                ClientSecrets = { new Secret("secret".Sha256()) },
                AllowedGrantTypes = GrantTypes.Code,
                AllowedScopes = new List<string> { "openid", "profile", "api" },
                RedirectUris = new List<string>
                {
                    "http://localhost:5236/signin-oidc"
                },
                PostLogoutRedirectUris = new List<string>
                {
                    "http://localhost:5236/signout-callback-oidc"
                }
            }
        };

        public static List<TestUser> Users => new List<TestUser>
        {
            new TestUser
            {
                SubjectId = "3fa85f64-5717-4562-b3fc-2c963f66af76",
                Username = "alice",
                Password = "alice",
                Claims =
                {
                    new Claim(JwtClaimTypes.Name, "Alice Smith"),
                    new Claim(JwtClaimTypes.GivenName, "Alice"),
                    new Claim(JwtClaimTypes.FamilyName, "Smith"),
                    new Claim(JwtClaimTypes.Email, "AliceSmith@email.com"),
                    new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean)
                }
            },
            new TestUser
            {
                SubjectId = "3fa85f64-5717-4562-b3fc-2c963f66af77",
                Username = "bob",
                Password = "bob",
                Claims =
                {
                    new Claim(JwtClaimTypes.Name, "Bob Smith"),
                    new Claim(JwtClaimTypes.GivenName, "Bob"),
                    new Claim(JwtClaimTypes.FamilyName, "Smith"),
                    new Claim(JwtClaimTypes.Email, "BobSmith@email.com"),
                    new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean)
                }
            }
        };
    }
}
