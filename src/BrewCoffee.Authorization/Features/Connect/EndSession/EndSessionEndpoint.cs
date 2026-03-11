using BrewCoffee.Authorization.Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Server.AspNetCore;
using ZedEndpoints.Abstractions;

namespace BrewCoffee.Authorization.Features.Connect.EndSession;

internal sealed class EndSessionEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/end-session", HandleAsync)
            .WithSummary("Encerrar sessão")
            .WithDescription(
                """
                Realiza o logout completo do usuário atual.
                Invoca o SignOutAsync do Identity para limpar o cookie de sessão local e, em seguida,
                encerra os esquemas de autenticação do Identity (ApplicationScheme) e do servidor OpenIddict,
                invalidando o token de acesso ativo.
                """)
            .Produces(StatusCodes.Status200OK);

    private static async Task<IResult> HandleAsync(
        HttpContext context,
        SignInManager<ApplicationUser> signInManager,
        CancellationToken cancellationToken)
    {
        await signInManager.SignOutAsync();

        return Results.SignOut(authenticationSchemes:
            [
                IdentityConstants.ApplicationScheme,
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
            ]
        );
    }
}