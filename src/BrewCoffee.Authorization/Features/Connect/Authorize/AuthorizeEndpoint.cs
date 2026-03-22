using System.Security.Claims;
using BrewCoffee.Authorization.Infrastructure.Extensions;
using BrewCoffee.Authorization.Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using ZedEndpoints.Abstractions;

namespace BrewCoffee.Authorization.Features.Connect.Authorize;

internal sealed class AuthorizeEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/authorize", HandleAsync).ExcludeFromDescription();

    private static async Task<IResult> HandleAsync(
        HttpContext context,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        var request = context.GetOpenIddictServerRequest();
        if (request is null) return Results.BadRequest("OpenIddict request not found.");
        var provider = request.GetParameter("provider")?.ToString();

        var localResult = await context.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        if (localResult.Succeeded)
        {
            var localUser = await userManager.FindByEmailAsync(
                localResult.Principal!.FindFirstValue(ClaimTypes.Email)!);

            if (localUser is null) return Results.Forbid();

            var localPrincipal = await signInManager.CreateUserPrincipalAsync(localUser);
            localPrincipal.ConfigurePrincipal();

            return Results.SignIn(
                principal: localPrincipal,
                authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
            );
        }

        var externalResult = await context.AuthenticateAsync(IdentityConstants.ExternalScheme);
        if (externalResult.Succeeded)
        {
            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info is null) return Results.Forbid();

            var user = await FindOrCreateUserAsync(info, userManager);
            var principal = await signInManager.CreateUserPrincipalAsync(user);
            principal.ConfigurePrincipal(request.GetScopes());

            return Results.SignIn(
                principal: principal,
                authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
            );
        }

        return RedirectToLogin(provider, signInManager, context);
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

        user = new ApplicationUser { UserName = email.Split('@')[0], Email = email, EmailConfirmed = true };
        await userManager.CreateAsync(user);
        await userManager.AddLoginAsync(user, info);

        return user;
    }

    private static IResult RedirectToLogin(
        string? provider,
        SignInManager<ApplicationUser> signInManager,
        HttpContext context)
    {
        var returnUrl = Uri.EscapeDataString(context.Request.GetEncodedPathAndQuery());

        if (string.IsNullOrWhiteSpace(provider) || provider == "local")
            return Results.Redirect($"/login?returnUrl={returnUrl}");

        var callbackUrl = $"/connect/external-callback?returnUrl={returnUrl}";
        var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, callbackUrl);
        return Results.Challenge(properties, [provider]);
    }
}