using System.Security.Claims;
using ZedEndpoints.Abstractions;

namespace BrewCoffee.BFF.Features.Auth;

internal sealed record UserResponse(
    string Id,
    string Email,
    string Name
);

internal sealed class MeEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/me", Handle)
            .RequireAuthorization()
            .WithSummary("Usuário autenticado")
            .WithDescription(
                """
                Retorna as informações do usuário autenticado.
                Lê as claims da sessão do BFF e retorna os dados básicos do usuário.
                """)
            .WithTags("Auth")
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

    private static IResult Handle(HttpContext context)
    {
        var user = context.User;

        var response = new UserResponse(
            Id: user.FindFirstValue("sub")!,
            Email: user.FindFirstValue("email")!,
            Name: user.FindFirstValue("name")!
        );

        return Results.Ok(response);
    }
}