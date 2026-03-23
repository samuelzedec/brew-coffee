using BrewCoffee.Shared.Abstractions.Services;
using ZedEndpoints.Abstractions;

namespace BrewCoffee.BFF.Features.Auth.Me;

internal sealed record UserResponse(
    string Id,
    string Email,
    string Name
);

internal sealed class MeEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/me", Handle);

    private static IResult Handle(ICurrentUserService currentUserService)
        => Results.Ok(new UserResponse(
            Id: currentUserService.Id,
            Email: currentUserService.Email,
            Name: currentUserService.Name
        ));
}