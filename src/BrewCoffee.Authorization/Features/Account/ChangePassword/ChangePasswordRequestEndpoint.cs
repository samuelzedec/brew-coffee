using BrewCoffee.Shared.Extensions;
using Mediator;
using ZedEndpoints.Abstractions;

namespace BrewCoffee.Authorization.Features.Account.ChangePassword;

internal sealed class ChangePasswordRequestEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPatch("change-password", HandleAsync);

    private static async Task<IResult> HandleAsync(
        ChangePasswordRequest request,
        ISender sender,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(request, cancellationToken);
        return result.ToActionResult();
    }
}