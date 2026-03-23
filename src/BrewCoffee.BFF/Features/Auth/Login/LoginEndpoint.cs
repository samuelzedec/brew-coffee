using Microsoft.AspNetCore.Authentication;
using ZedEndpoints.Abstractions;

namespace BrewCoffee.BFF.Features.Auth.Login;

internal sealed class LoginEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/login", Handle)
            .AllowAnonymous();

    private static IResult Handle(
        string route,
        IConfiguration configuration)
    {
        var returnUrl = $"{configuration["FrontendUrl"]}/{route}";
        var properties = new AuthenticationProperties { RedirectUri = returnUrl };
        return Results.Challenge(properties);
    }
}