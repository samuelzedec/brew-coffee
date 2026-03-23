using BrewCoffee.Shared.Extensions;
using Mediator;
using ZedEndpoints.Abstractions;

namespace BrewCoffee.Authorization.Features.Account.HasPassword;

internal sealed class HasPasswordRequestEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("password/exists", HandleAsync);

    private static async Task<IResult> HandleAsync(
        ISender sender,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new HasPasswordRequest(), cancellationToken);
        return result.ToActionResult();
    }
}