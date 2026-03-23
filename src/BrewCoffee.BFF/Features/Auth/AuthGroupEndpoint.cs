using BrewCoffee.BFF.Features.Auth.Login;
using BrewCoffee.BFF.Features.Auth.Logout;
using BrewCoffee.BFF.Features.Auth.Me;
using ZedEndpoints.Abstractions;
using ZedEndpoints.Extensions;

namespace BrewCoffee.BFF.Features.Auth;

internal sealed class AuthGroupEndpoint : IEndpointGroup
{
    public void MapGroup(IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/auth")
            .RequireAuthorization();

        group
            .MapEndpoint<LoginEndpoint>()
            .MapEndpoint<LogoutEndpoint>()
            .MapEndpoint<MeEndpoint>();
    }
}