using BrewCoffee.Authorization.Infrastructure.Persistence.Identity;
using BrewCoffee.Authorization.Infrastructure.Extensions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using ZedEndpoints.Abstractions;

namespace BrewCoffee.Authorization.Features.Connect.Token;

internal sealed class TokenEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/token", HandleAsync).ExcludeFromDescription();

    private static async Task<IResult> HandleAsync(
        HttpContext context,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        var request = context.GetOpenIddictServerRequest();

        if (request is null)
            return Results.BadRequest("OpenIddict request not found.");

        return !request.IsAuthorizationCodeGrantType() && !request.IsRefreshTokenGrantType()
            ? Results.BadRequest("Invalid grant type.")
            : await IssueTokenAsync(context, request, signInManager, userManager);
    }

    private static async Task<IResult> IssueTokenAsync(
        HttpContext context,
        OpenIddictRequest request,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        var result = await context.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        if (!result.Succeeded) return Results.Forbid();

        var userId = result.Principal?.GetClaim(OpenIddictConstants.Claims.Subject);
        if (string.IsNullOrWhiteSpace(userId)) return Results.Forbid();

        var user = await userManager.FindByIdAsync(userId);
        if (user is null || !await signInManager.CanSignInAsync(user))
            return Results.Forbid();

        var principal = await signInManager.CreateUserPrincipalAsync(user);
        principal.ConfigurePrincipal(request.GetScopes());

        return Results.SignIn(
            principal: principal,
            authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
        );
    }
}