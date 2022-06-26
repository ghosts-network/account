using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Duende.IdentityServer.Services;
using GhostNetwork.Account.Web.Services;
using GhostNetwork.Account.Web.Services.EmailSender;
using GhostNetwork.AspNetCore.Identity.Mongo;
using GhostNetwork.Profiles.Api;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;

namespace GhostNetwork.Account.Web
{
    public class Startup
    {
        private const string DefaultDbName = "account";

        private IWebHostEnvironment Environment { get; }

        private IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            if (Configuration["EMAIL_SENDER"] == "SMTP")
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
            else
            {
                services.AddScoped<IEmailSender, NullEmailSender>();
            }

            services.AddScoped<IDefaultClientProvider>(_ => new DefaultClientProvider(Configuration["DEFAULT_CLIENT"]));

            services.AddScoped<IProfilesApi>(_ => new ProfilesApi(Configuration["PROFILES_ADDRESS"]));

            services.AddControllersWithViews();

            var mongoUrl = MongoUrl.Create(Configuration["MONGO_ADDRESS"]);
            services.AddIdentity<IdentityUser, IdentityRole>(options =>
                {
                    options.User.RequireUniqueEmail = true;
                    options.SignIn.RequireConfirmedEmail = true;
                })
                .AddMongoDbStores<IdentityDbContext<string>>(new MongoOptions(MongoClientSettings.FromUrl(mongoUrl), mongoUrl.DatabaseName ?? DefaultDbName))
                .AddDefaultTokenProviders();

            var builder = services.AddIdentityServer(options =>
            {
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
                .AddInMemoryApiResources(Config.ApiResources)
                .AddInMemoryClients(Config.Clients)
                .AddAspNetIdentity<IdentityUser>();

            if (Configuration["SINGING_TYPE"] == "X509")
            {
                var path = Configuration["X509_PATH"];
                var password = Configuration["X509_PASSWORD"];
                var algorithm = Configuration["X509_ALGORITHM"] ?? "RS256";
                var certificate = new X509Certificate2(path, password);

                builder.AddSigningCredential(certificate, algorithm);
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
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
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

            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

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
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }

        private string[] GetAllowOrigins()
        {
            return Configuration.GetValue<string>("ALLOWED_HOSTS")?.Split(',').ToArray() ?? Array.Empty<string>();
        }
    }
}