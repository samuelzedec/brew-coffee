using System.Security.Claims;
using BrewCoffee.Authorization.Infrastructure.Persistence.Identity;
using BrewCoffee.Authorization.Infrastructure.Extensions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Server.AspNetCore;
using ZedEndpoints.Abstractions;

namespace BrewCoffee.Authorization.Features.Connect.Authorize;

internal sealed class AuthorizeEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/authorize", HandleAsync)
            .ExcludeFromDescription();


    private static async Task<IResult> HandleAsync(
        HttpContext context,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        var request = context.GetOpenIddictServerRequest();
        if (request is null) return Results.BadRequest("OpenIddict request not found.");

        var result = await context.AuthenticateAsync(IdentityConstants.ExternalScheme);
        if (!result.Succeeded)
        {
            var provider = request
                .GetParameter("provider")?.ToString() ?? "Google";

            var properties = signInManager.ConfigureExternalAuthenticationProperties(
                provider,
                "/connect/external-callback");

            return Results.Challenge(properties, [provider]);
        }

        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info is null) return Results.Forbid();

        var user = await FindOrCreateUserAsync(info, userManager);
        var principal = await signInManager.CreateUserPrincipalAsync(user);
        principal.ConfigurePrincipal();

        return Results.SignIn(
            principal: principal,
            authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
        );
    }

    /// <summary>
    /// Localiza ou cria um usuário na base de dados utilizando informações obtidas de login externo.
    /// </summary>
    /// <param name="info">
    /// Informações de login externo contendo detalhes como provedor, chave do provedor e reivindicações do usuário.
    /// </param>
    /// <param name="userManager">
    /// Instância do <see cref="UserManager&lt;TUser&gt;"/> utilizada para gerenciar as operações relacionadas ao usuário.
    /// </param>
    /// <returns>
    /// Retorna uma instância de <see cref="ApplicationUser"/> correspondente ao usuário encontrado ou criado.
    /// </returns>
    private static async Task<ApplicationUser> FindOrCreateUserAsync(
        ExternalLoginInfo info,
        UserManager<ApplicationUser> userManager)
    {
        var user = await userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
        if (user is not null) return user;

        var email = info.Principal.FindFirstValue(ClaimTypes.Email)!;
        user = await userManager.FindByEmailAsync(email);

        if (user is not null)
        {
            await userManager.AddLoginAsync(user, info);
            return user;
        }

        user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
        await userManager.CreateAsync(user);
        await userManager.AddLoginAsync(user, info);

        return user;
    }
}