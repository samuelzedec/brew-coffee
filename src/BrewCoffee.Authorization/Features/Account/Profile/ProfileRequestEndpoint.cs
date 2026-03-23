using BrewCoffee.Shared.Extensions;
using Mediator;
using ZedEndpoints.Abstractions;

namespace BrewCoffee.Authorization.Features.Account.Profile;

internal sealed class ProfileRequestEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPatch("profile", HandleAsync);

    private static async Task<IResult> HandleAsync(
        ProfileRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(request, cancellationToken);
        return result.ToActionResult();
    }
}