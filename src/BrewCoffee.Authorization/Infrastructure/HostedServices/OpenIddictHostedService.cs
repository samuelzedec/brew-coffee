using BrewCoffee.Authorization.Infrastructure.Extensions;
using OpenIddict.Abstractions;

namespace BrewCoffee.Authorization.Infrastructure.HostedServices;

/// <summary>
/// Representa um serviço hospedado do OpenIddict para realizar o seed inicial
/// de escopos e clientes na aplicação.
/// </summary>
internal sealed class OpenIddictHostedService(
    IServiceProvider serviceProvider,
    IConfiguration configuration
) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        await SeedScopesAsync(scope.ServiceProvider, cancellationToken);
        await SeedApplicationsAsync(scope.ServiceProvider, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    /// <summary>
    /// Cria e registra o escopo padrão do OpenIddict caso ele ainda não exista.
    /// </summary>
    /// <param name="services">
    /// Um <see cref="IServiceProvider"/> usado para acessar os serviços necessários.
    /// </param>
    /// <param name="cancellationToken">
    /// Um token para monitorar solicitações de cancelamento da operação.
    /// </param>
    /// <returns>
    /// Uma <see cref="Task"/> que representa a operação de seeding do escopo.
    /// </returns>
    private static async Task SeedScopesAsync(
        IServiceProvider services,
        CancellationToken cancellationToken)
    {
        var scopeManager = services.GetRequiredService<IOpenIddictScopeManager>();

        if (await scopeManager.FindByNameAsync("api", cancellationToken) is null)
            await scopeManager.CreateAsync(GetApiScopeDescriptor(), cancellationToken);
    }

    /// <summary>
    /// Cria e registra os clientes padrão do OpenIddict caso eles ainda não existam.
    /// </summary>
    /// <param name="services">
    /// Um <see cref="IServiceProvider"/> usado para acessar os serviços necessários.
    /// </param>
    /// <param name="cancellationToken">
    /// Um token para monitorar solicitações de cancelamento da operação.
    /// </param>
    /// <returns>
    /// Uma <see cref="Task"/> que representa a operação de seeding dos clientes.
    /// </returns>
    private async Task SeedApplicationsAsync(
        IServiceProvider services,
        CancellationToken cancellationToken)
    {
        var manager = services.GetRequiredService<IOpenIddictApplicationManager>();

        if (await manager.FindByClientIdAsync("brewcoffee-bff", cancellationToken) is null)
            await manager.CreateAsync(GetBffDescriptor(), cancellationToken);

        if (await manager.FindByClientIdAsync("worker-service", cancellationToken) is null)
            await manager.CreateAsync(GetWorkerDescriptor(), cancellationToken);
    }

    private static OpenIddictScopeDescriptor GetApiScopeDescriptor()
        => new() { Name = "api", Description = "CoffeeAgent API Access", Resources = { "coffee_agent_api" } };

    private OpenIddictApplicationDescriptor GetBffDescriptor()
    {
        (string clientId, string clientSecret) = configuration.GetProviderAuth("BFF");
        return new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
            ClientType = OpenIddictConstants.ClientTypes.Confidential,
            DisplayName = "BrewCoffee BFF",
            RedirectUris = { new Uri(configuration.GetOpenIddictClientConfig("BFF", "CallbackUri")) },
            PostLogoutRedirectUris = { new Uri(configuration.GetOpenIddictClientConfig("BFF", "LogoutUri")) },
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Authorization,
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.Endpoints.EndSession,
                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                OpenIddictConstants.Permissions.GrantTypes.Password,
                OpenIddictConstants.Permissions.ResponseTypes.Code,
                OpenIddictConstants.Permissions.Prefixes.Scope + "openid",
                OpenIddictConstants.Permissions.Prefixes.Scope + "email",
                OpenIddictConstants.Permissions.Prefixes.Scope + "profile",
                OpenIddictConstants.Permissions.Prefixes.Scope + "api",
                OpenIddictConstants.Permissions.Prefixes.Scope + "roles",
                OpenIddictConstants.Permissions.Prefixes.Scope + "offline_access"
            }
        };
    }

    private OpenIddictApplicationDescriptor GetWorkerDescriptor()
    {
        (string clientId, string clientSecret) = configuration.GetProviderAuth("M2M");
        return new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
            ClientType = OpenIddictConstants.ClientTypes.Confidential,
            DisplayName = "CoffeeAgent Worker Service",
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                OpenIddictConstants.Permissions.Prefixes.Scope + "api",
            }
        };
    }
}