using BrewCoffee.Authorization.Infrastructure.Extensions;
using BrewCoffee.Authorization.Infrastructure.HostedServices;
using BrewCoffee.Authorization.Infrastructure.Persistence;
using BrewCoffee.Authorization.Infrastructure.Persistence.Identity;
using BrewCoffee.Authorization.Infrastructure.Services;
using BrewCoffee.Shared.Abstractions.Services;
using BrewCoffee.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using Serilog;
using Serilog.Events;

namespace BrewCoffee.Authorization.Infrastructure.Setups;

internal static class BuilderSetup
{
    extension(WebApplicationBuilder builder)
    {
        /// <summary>
        /// Configura os componentes principais do aplicativo, incluindo documentação da API, opções, agentes, banco de dados,
        /// medição com validação, logger, gerenciamento de exceções, serviços auxiliares e repositórios.
        /// </summary>
        /// <remarks>
        /// Este método realiza o registro de várias dependências e serviços essenciais utilizados pela aplicação.
        /// Ele organiza de maneira modular diferentes configurações para facilitar a manutenção e a escalabilidade do sistema.
        /// </remarks>
        public void Configure()
        {
            builder.ConfigureDbContext();
            builder.ConfigureIdentity();
            builder.ConfigureOpenIddict();
            builder.ConfigureAuthentication();
            builder.ConfigureLogger();
            builder.ConfigureExceptionHandling();
            builder.ConfigureServices();
            builder.ConfigureRazorPages();
        }

        private void ConfigureDbContext()
        {
            var isDevelopment = builder.Environment
                .IsDevelopment();

            builder.Services.AddDbContext<BrewCoffeeAuthDbContext>(options => options
                .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), b => b
                    .MigrationsAssembly(typeof(BrewCoffeeAuthDbContext).Assembly.FullName)
                    .MigrationsHistoryTable("__EFMigrationsHistory", "identity"))
                .UseOpenIddict<Guid>()
                .EnableServiceProviderCaching()
                .EnableSensitiveDataLogging(isDevelopment)
                .EnableDetailedErrors(isDevelopment));
        }

        private void ConfigureLogger()
        {
            const string logStructure = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Mvc", LogEventLevel.Warning)
                .WriteTo.Console(outputTemplate: logStructure)
                .CreateLogger();

            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog();
        }

        private void ConfigureExceptionHandling()
        {
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();
        }

        private void ConfigureIdentity()
        {
            builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
                    options.Stores.SchemaVersion = IdentitySchemaVersions.Version3)
                .AddEntityFrameworkStores<BrewCoffeeAuthDbContext>()
                .AddDefaultTokenProviders();
        }

        private void ConfigureOpenIddict()
        {
            builder.Services.AddOpenIddict()
                .AddCore(options => options
                    .UseEntityFrameworkCore()
                    .UseDbContext<BrewCoffeeAuthDbContext>()
                    .ReplaceDefaultEntities<Guid>()
                )
                .AddServer(options =>
                {
                    // endpoints que o openiddict ficará de olho para cada ação
                    options
                        .SetAuthorizationEndpointUris("/connect/authorize")
                        .SetTokenEndpointUris("/connect/token")
                        .SetEndSessionEndpointUris("/connect/end-session")
                        .SetUserInfoEndpointUris("/connect/userinfo");

                    options
                        .AllowClientCredentialsFlow()
                        .AllowAuthorizationCodeFlow()
                        .AllowRefreshTokenFlow();

                    options.RegisterScopes(
                        OpenIddictConstants.Scopes.OpenId,
                        OpenIddictConstants.Scopes.Email,
                        OpenIddictConstants.Scopes.Profile,
                        OpenIddictConstants.Scopes.Roles,
                        OpenIddictConstants.Scopes.OfflineAccess
                    );

                    // Força PKCE (Proof Key for Code Exchange) no Authorization Code Flow
                    options.RequireProofKeyForCodeExchange();

                    options
                        .AddDevelopmentEncryptionCertificate()
                        .AddDevelopmentSigningCertificate();

                    // Passthrough: delega o processamento dos endpoints para controllers/handlers customizados
                    // Sem isso o OpenIddict tentaria processar automaticamente sem permitir lógica customizada
                    options
                        .UseAspNetCore()
                        .EnableAuthorizationEndpointPassthrough()
                        .EnableTokenEndpointPassthrough()
                        .EnableEndSessionEndpointPassthrough()
                        .EnableUserInfoEndpointPassthrough();
                })
                .AddValidation(options =>
                {
                    options.UseLocalServer();
                    options.UseAspNetCore();
                });
        }

        private void ConfigureAuthentication()
        {
            builder.Services.AddAuthentication()
                .AddGoogle(options =>
                {
                    var client = builder.Configuration.GetProviderAuth("Google");
                    options.ClientId = client.Id;
                    options.ClientSecret = client.Secret;
                })
                .AddMicrosoftAccount(options =>
                {
                    var client = builder.Configuration.GetProviderAuth("Microsoft");
                    options.ClientId = client.Id;
                    options.ClientSecret = client.Secret;
                });

            builder.Services.AddAuthorization();
        }

        private void ConfigureServices()
        {
            builder.Services.AddTransient<ICurrentUserService, CurrentUserService>();
            builder.Services.AddHostedService<OpenIddictHostedService>();
        }

        private void ConfigureRazorPages()
        {
            builder.Services.AddRazorPages();
        }
    }
}