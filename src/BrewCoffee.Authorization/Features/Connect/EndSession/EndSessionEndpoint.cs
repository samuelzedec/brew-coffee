using BrewCoffee.Authorization.Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Server.AspNetCore;
using ZedEndpoints.Abstractions;

namespace BrewCoffee.Authorization.Features.Connect.EndSession;

internal sealed class EndSessionEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/end-session", HandleAsync)
            .WithSummary("End Session")
            .WithDescription("Logs out the current user and terminates the session.")
            .Produces(StatusCodes.Status200OK);

    private static async Task<IResult> HandleAsync(
        HttpContext context,
        SignInManager<ApplicationUser> signInManager,
        CancellationToken cancellationToken)
    {
        await signInManager.SignOutAsync();

        return Results.SignOut(authenticationSchemes:
            [
                IdentityConstants.ApplicationScheme,
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
            ]
        );
    }
}