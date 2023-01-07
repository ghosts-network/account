using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using GhostNetwork.Account.Mongo;
using GhostNetwork.Account.Web.Services.EmailSender;
using GhostNetwork.Account.Web.Services.EmailSender.NotificationsService;
using GhostNetwork.Account.Web.Services.EmailSender.Smtp;
using GhostNetwork.Account.Web.Services.OAuth.Clients;
using GhostNetwork.AspNetCore.Identity.Mongo;
using GhostNetwork.Profiles.Api;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;

namespace GhostNetwork.Account.Web
{
    public class Startup
    {
        private const string DefaultDbName = "account";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            if (Configuration["EMAIL_SENDER"].Equals("smtp", StringComparison.InvariantCultureIgnoreCase))
            {
                services.AddScoped<IEmailSender, SmtpEmailSender>(_ =>
                {
                    var config = new SmtpClientConfiguration(
                        Configuration["SMTP_HOST"],
                        Configuration.GetValue<int>("SMTP_POST"),
                        Configuration.GetValue<bool>("SMTP_SSL_ENABLED"),
                        Configuration["SMTP_USERNAME"],
                        Configuration["SMTP_PASSWORD"],
                        Configuration["SMTP_DISPLAY_NAME"],
                        Configuration["SMTP_EMAIL"]);
                    return new SmtpEmailSender(config);
                });
            }
            else if (Configuration["EMAIL_SENDER"].Equals("service", StringComparison.InvariantCultureIgnoreCase))
            {
                services.AddScoped<NotificationsSender>()
                    .AddHttpClient<NotificationsSender>(c => c.BaseAddress = new Uri(Configuration["NOTIFICATIONS_ADDRESS"]));

                services.AddScoped<IEmailSender, NotificationsSender>(provider =>
                    provider.GetRequiredService<NotificationsSender>());
            }
            else
            {
                services.AddScoped<IEmailSender, NullEmailSender>();
            }

            services.AddScoped<IDefaultClientProvider>(_ => new DefaultClientProvider(Configuration["DEFAULT_CLIENT"]));

            services.AddScoped<IProfilesApi>(_ => new ProfilesApi(Configuration["PROFILES_ADDRESS"]));

            services.AddRazorPages();
            services.AddControllersWithViews();

            AddMongo(services);
            services.AddIdentity<IdentityUser, IdentityRole>(options =>
                {
                    options.User.RequireUniqueEmail = true;
                    options.SignIn.RequireConfirmedEmail = true;
                })
                .AddMongoDbStores<IdentityDbContext<string>>()
                .AddDefaultTokenProviders();

            services.AddScoped<ClientsStorage>();

            var builder = services.AddIdentityServer(options =>
            {
                options.IssuerUri = Configuration["ISSUER_URI"];
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                // see https://identityserver4.readthedocs.io/en/latest/topics/resources.html
                options.EmitStaticAudienceClaim = true;

                // key management options
                options.KeyManagement.Enabled = true;
            })
                .AddInMemoryIdentityResources(Config.IdentityResources)
                .AddInMemoryApiScopes(Config.ApiScopes)
                .AddInMemoryApiResources(Config.ApiResources);

            if (Configuration["OAUTH_CLIENTS_SOURCE"] == "file")
            {
                services.AddScoped(provider => new ComposedClientStorage(
                    new InMemoryClientStorage(GetClients()),
                    new MongoClientStorage(provider.GetRequiredService<ClientsStorage>())));
                builder.AddClientStore<ComposedClientStorage>();
            }
            else
            {
                services.AddScoped(provider => new ComposedClientStorage(
                    new InMemoryClientStorage(Config.Clients),
                    new MongoClientStorage(provider.GetRequiredService<ClientsStorage>())));
                builder.AddClientStore<ComposedClientStorage>();
            }

            builder.AddAspNetIdentity<IdentityUser>();

            if (Configuration["SINGING_TYPE"] == "X509")
            {
                var path = Configuration["X509_PATH"];
                var password = Configuration["X509_PASSWORD"];
                var algorithm = Configuration["X509_ALGORITHM"] ?? "RS256";
                var certificate = new X509Certificate2(path, password);

                builder.AddSigningCredential(certificate, algorithm);
            }
            else if (Configuration["SINGING_TYPE"] == "Developer")
            {
                builder.AddDeveloperSigningCredential();
            }
            else
            {
                throw new InvalidOperationException("Invalid signing credential type");
            }

            builder.Services.ConfigureExternalCookie(options =>
            {
                options.Cookie.IsEssential = true;
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            });

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.IsEssential = true;
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            });

            services.AddAuthentication();
        }

        public void Configure(IApplicationBuilder app, IServerUrls serverUrls)
        {
            if (!string.IsNullOrEmpty(Configuration["Host"]))
            {
                app.Use(async (_, next) =>
                {
                    serverUrls.Origin = Configuration["Host"];
                    await next();
                });
            }

            app.UseDeveloperExceptionPage();
            app.UseStaticFiles();

            app.UseCors(builder =>
            {
                var allowOrigins = GetAllowOrigins();
                builder = allowOrigins.Any()
                    ? builder.WithOrigins(allowOrigins)
                    : builder.AllowAnyOrigin();

                builder
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });

            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseStatusCodePages();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapRazorPages();
            });
        }

        private void AddMongo(IServiceCollection services)
        {
            var mongoUrl = MongoUrl.Create(Configuration["MONGO_ADDRESS"]);
            services.AddSingleton(provider =>
            {
                var settings = MongoClientSettings.FromUrl(mongoUrl);
                settings.ClusterConfigurator = cb =>
                {
                    cb.Subscribe<CommandStartedEvent>(_ =>
                    {
                        var logger = provider.GetRequiredService<ILogger<MongoDbContext>>();
                        using var scope = logger.BeginScope(new Dictionary<string, object>
                        {
                            ["type"] = "outgoing:mongodb"
                        });

                        logger.LogInformation("Mongodb query started");
                    });

                    cb.Subscribe<CommandSucceededEvent>(e =>
                    {
                        var logger = provider.GetRequiredService<ILogger<MongoDbContext>>();
                        using var scope = logger.BeginScope(new Dictionary<string, object>
                        {
                            ["type"] = "outgoing:mongodb",
                            ["elapsedMilliseconds"] = e.Duration.Milliseconds
                        });

                        logger.LogInformation("Mongodb query finished");
                    });

                    cb.Subscribe<CommandFailedEvent>(e =>
                    {
                        var logger = provider.GetRequiredService<ILogger<MongoDbContext>>();
                        using var scope = logger.BeginScope(new Dictionary<string, object>
                        {
                            ["type"] = "outgoing:mongodb",
                            ["elapsedMilliseconds"] = e.Duration.Milliseconds
                        });

                        logger.LogInformation("Mongodb query failed");
                    });
                };
                return new MongoClient(settings);
            });

            services.AddScoped(provider => provider.GetRequiredService<MongoClient>()
                .GetDatabase(mongoUrl.DatabaseName ?? DefaultDbName));
            services.AddScoped<MongoDbContext>();
        }

        private string[] GetAllowOrigins()
        {
            return Configuration.GetValue<string>("ALLOWED_HOSTS")?.Split(',').ToArray() ?? Array.Empty<string>();
        }

        private IEnumerable<Client> GetClients()
        {
            var section = new ConfigurationBuilder()
                .AddJsonFile(Configuration.GetValue("OAUTH_CLIENTS_FILE", "./clients.json"))
                .Build()
                .GetRequiredSection("clients");

            var clients = new List<Client>();
            section.Bind(clients);

            return clients;
        }
    }
}