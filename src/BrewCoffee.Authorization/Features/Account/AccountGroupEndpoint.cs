using BrewCoffee.Authorization.Features.Account.ChangePassword;
using BrewCoffee.Authorization.Features.Account.HasPassword;
using BrewCoffee.Authorization.Features.Account.Profile;
using ZedEndpoints.Abstractions;
using ZedEndpoints.Extensions;

namespace BrewCoffee.Authorization.Features.Account;

internal sealed class AccountGroupEndpoint : IEndpointGroup
{
    public void MapGroup(IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/account")
            .RequireAuthorization();

        group
            .MapEndpoint<ChangePasswordRequestEndpoint>()
            .MapEndpoint<ProfileRequestEndpoint>()
            .MapEndpoint<HasPasswordRequestEndpoint>();
    }
}