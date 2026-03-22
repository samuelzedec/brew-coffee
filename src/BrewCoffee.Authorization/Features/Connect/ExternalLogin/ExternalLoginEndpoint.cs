using BrewCoffee.Authorization.Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using ZedEndpoints.Abstractions;

namespace BrewCoffee.Authorization.Features.Connect.ExternalLogin;

internal sealed class ExternalLoginEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/external-login", Handle)
            .AllowAnonymous();

    private static IResult Handle(
        string provider,
        string returnUrl,
        SignInManager<ApplicationUser> signInManager)
    {
        var callbackUrl = $"/connect/external-callback?returnUrl={Uri.EscapeDataString(returnUrl)}";
        var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, callbackUrl);
        return Results.Challenge(properties, [provider]);
    }
}