using ZedEndpoints.Abstractions;
using ZedEndpoints.Extensions;

namespace BrewCoffee.BFF.Features.Auth;

internal sealed class AuthGroupEndpoint : IEndpointGroup
{
    public void MapGroup(IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/auth")
            .WithTags("Auth");

        group
            .MapEndpoint<LoginEndpoint>()
            .MapEndpoint<LogoutEndpoint>()
            .MapEndpoint<MeEndpoint>();
    }
}