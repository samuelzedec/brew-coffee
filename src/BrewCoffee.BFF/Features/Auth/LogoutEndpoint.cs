using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using ZedEndpoints.Abstractions;

namespace BrewCoffee.BFF.Features.Auth;

internal sealed class LogoutEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/logout", HandleAsync)
            .RequireAuthorization()
            .WithSummary("Logout")
            .WithDescription(
                """
                Encerra a sessão do usuário autenticado.
                Remove o cookie de sessão e redireciona para o Authorization Server
                para encerrar a sessão lá também.
                """)
            .WithTags("Auth")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

    private static async Task HandleAsync(
        HttpContext context,
        string? returnUrl)
    {
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        await context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme,
            new AuthenticationProperties { RedirectUri = returnUrl ?? "http://localhost:4200" });
    }
}