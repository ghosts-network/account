using GhostNetwork.Account.Web.Services;
using GhostNetwork.Account.Web.Services.EmailSender;
using GhostNetwork.AspNetCore.Identity.Mongo;
using GhostNetwork.Profiles.Api;
using IdentityServer4.Extensions;
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
        
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            if (Configuration["EMAIL_SENDER"] == "SMTP")
            {
                services.AddScoped<IEmailSender, SmtpEmailSender>(_ =>
                {
                    var config = new SmtpClientConfiguration(Configuration["SMTP_HOST"],
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
            })
                // .AddTestUsers(Config.Users)
                .AddInMemoryIdentityResources(Config.IdentityResources)
                .AddInMemoryApiScopes(Config.ApiScopes)
                .AddInMemoryApiResources(Config.ApiResources)
                .AddInMemoryClients(Config.Clients)
                .AddAspNetIdentity<IdentityUser>();

            builder.Services.ConfigureExternalCookie(options => {
                options.Cookie.IsEssential = true;
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            builder.Services.ConfigureApplicationCookie(options => {
                options.Cookie.IsEssential = true;
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            // not recommended for production - you need to store your key material somewhere secure
            builder.AddDeveloperSigningCredential();

            services.AddAuthentication();
        }

        public void Configure(IApplicationBuilder app)
        {
            if (!string.IsNullOrEmpty(Configuration["Host"]))
            {
                app.Use(async (ctx, next) =>
                {
                    ctx.SetIdentityServerOrigin(Configuration["Host"]);
                    await next();
                });
            }

            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}