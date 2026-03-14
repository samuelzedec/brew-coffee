using BrewCoffee.Shared.Common;
using BrewCoffee.Shared.Extensions;
using Mediator;
using ZedEndpoints.Abstractions;

namespace BrewCoffee.Authorization.Features.Account.Register;

internal sealed class RegisterEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/register", HandleAsync)
            .WithSummary("Registrar usuário")
            .WithDescription(
                """
                Cria uma nova conta de usuário local com nome de usuário, e-mail e senha.
                Antes de criar o usuário, valida se já existe uma conta com o mesmo e-mail ou nome de usuário.
                Em caso de sucesso, retorna o ID e o e-mail do usuário criado, junto ao cabeçalho Location apontando para o recurso.
                """)
            .Produces<RegisterResponse>(StatusCodes.Status201Created)
            .Produces<Error>(StatusCodes.Status400BadRequest)
            .Produces<Error>(StatusCodes.Status409Conflict)
            .Produces<Error>(StatusCodes.Status500InternalServerError);

    private static async Task<IResult> HandleAsync(
        RegisterRequest request,
        ISender sender,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(request, cancellationToken);
        return result.ToActionResult($"register/{result.Value?.Id}");
    }
}