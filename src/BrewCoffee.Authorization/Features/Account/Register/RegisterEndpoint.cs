using BrewCoffee.Shared.Extensions;
using Mediator;
using ZedEndpoints.Abstractions;

namespace BrewCoffee.Authorization.Features.Account.Register;

internal sealed class RegisterEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/register", HandleAsync);

    private static async Task<IResult> HandleAsync(
        RegisterRequest request,
        ISender sender,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(request, cancellationToken);
        return result.ToActionResult($"register/{result.Value?.Id}");
    }
}