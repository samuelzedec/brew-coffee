using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using ZedEndpoints.Abstractions;

namespace BrewCoffee.BFF.Features.Auth.Logout;

internal sealed class LogoutEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/logout", HandleAsync);

    private static async Task HandleAsync(
        HttpContext context,
        IConfiguration configuration)
    {
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme,
            new AuthenticationProperties { RedirectUri = configuration["FrontendUrl"] });
    }
}