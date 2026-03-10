using BrewCoffee.Authorization.Infrastructure.HostedServices;
using BrewCoffee.Authorization.Infrastructure.Persistence;
using BrewCoffee.Authorization.Infrastructure.Persistence.Identity;
using BrewCoffee.Authorization.Infrastructure.Extensions;
using BrewCoffee.Shared.Behaviors;
using BrewCoffee.Shared.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
            builder.ConfigureApiDocumentation();
            builder.ConfigureDbContext();
            builder.ConfigureIdentity();
            builder.ConfigureOpenIddict();
            builder.ConfigureAuthentication();
            builder.ConfigureMediatorWithValidation();
            builder.ConfigureLogger();
            builder.ConfigureExceptionHandling();
            builder.ConfigureHostedServices();
        }

        private void ConfigureApiDocumentation()
        {
            builder.Services.AddOpenApi();
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

        private void ConfigureMediatorWithValidation()
        {
            builder.Services.AddMediator(options =>
            {
                options.ServiceLifetime = ServiceLifetime.Scoped;
                options.PipelineBehaviors = [typeof(ValidationBehavior<,>)];
            });

            builder.Services.AddValidatorsFromAssembly(
                typeof(Program).Assembly,
                includeInternalTypes: true
            );
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
                        .AllowClientCredentialsFlow() // > Fluxo M2M: serviços autenticando com client_id + client_secret sem usuário humano
                        .AllowAuthorizationCodeFlow() // > Fluxo para usuários humanos fazendo login via Google, GitHub ou Microsoft
                        .AllowRefreshTokenFlow(); // > Permite renovar o access_token sem o usuário logar de novo

                    // Força PKCE no Authorization Code Flow
                    // O frontend gera um code_verifier secreto que é validado na troca do token
                    // Impede que alguém intercepte o code e roube a sessão
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

        private void ConfigureHostedServices()
        {
            builder.Services.AddHostedService<OpenIddictHostedService>();
        }
    }
}