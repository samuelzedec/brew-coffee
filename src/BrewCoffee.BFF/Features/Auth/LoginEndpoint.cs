using Microsoft.AspNetCore.Authentication;
using ZedEndpoints.Abstractions;

namespace BrewCoffee.BFF.Features.Auth;

internal sealed class LoginEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/login", Handle)
            .AllowAnonymous()
            .WithTags("Auth")
            .Produces(StatusCodes.Status302Found);

    private static IResult Handle(string? returnUrl)
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = returnUrl ?? "http://localhost:4200"
        };

        return Results.Challenge(properties);
    }
}