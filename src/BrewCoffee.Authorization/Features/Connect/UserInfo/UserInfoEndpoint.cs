using BrewCoffee.Authorization.Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using ZedEndpoints.Abstractions;

namespace BrewCoffee.Authorization.Features.Connect.UserInfo;

internal sealed class UserInfoEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/userinfo", HandleAsync)
            .RequireAuthorization()
            .WithSummary("User Info")
            .WithDescription("Returns the claims of the authenticated user.")
            .Produces<Dictionary<string, object>>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

    private static async Task<IResult> HandleAsync(
        HttpContext context,
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        var result = await context.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        if (!result.Succeeded) return Results.Forbid();

        var userId = result.Principal?.GetClaim(OpenIddictConstants.Claims.Subject);
        if (string.IsNullOrWhiteSpace(userId)) return Results.Forbid();

        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return Results.Forbid();

        var claims = new Dictionary<string, object>
        {
            [OpenIddictConstants.Claims.Subject] = user.Id.ToString(),
            [OpenIddictConstants.Claims.Email] = user.Email!,
            [OpenIddictConstants.Claims.EmailVerified] = user.EmailConfirmed,
            [OpenIddictConstants.Claims.Name] = user.UserName!
        };

        return Results.Ok(claims);
    }
}