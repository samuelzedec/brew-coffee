using BrewCoffee.Authorization.Features.Account.Register;
using ZedEndpoints.Abstractions;
using ZedEndpoints.Extensions;

namespace BrewCoffee.Authorization.Features.Account;

internal sealed class AccountGroupEndpoint : IEndpointGroup
{
    public void MapGroup(IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/account")
            .WithTags("Account");

        group
            .MapEndpoint<RegisterEndpoint>();
    }
}