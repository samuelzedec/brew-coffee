using BrewCoffee.Authorization.Infrastructure.Persistence.Identity;
using BrewCoffee.Shared.Abstractions.Requests;
using BrewCoffee.Shared.Abstractions.Services;
using BrewCoffee.Shared.Common;
using Microsoft.AspNetCore.Identity;

namespace BrewCoffee.Authorization.Features.Account.HasPassword;

internal sealed class HasPasswordRequestHandler(
    UserManager<ApplicationUser> userManager,
    ICurrentUserService currentUserService)
    : IRequestHandler<HasPasswordRequest, HasPasswordRequestResponse>
{
    public async ValueTask<Result<HasPasswordRequestResponse>> Handle(
        HasPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(currentUserService.Id);
        if (user is null)
            return Error.NotFound("Usuário não encontrado.");

        var hasPassword = await userManager.HasPasswordAsync(user);
        return new HasPasswordRequestResponse(hasPassword);
    }
}