using BrewCoffee.Authorization.Infrastructure.Persistence.Identity;
using BrewCoffee.Shared.Abstractions.Request;
using BrewCoffee.Shared.Abstractions.Services;
using BrewCoffee.Shared.Common;
using Microsoft.AspNetCore.Identity;

namespace BrewCoffee.Authorization.Features.Account.ChangePassword;

internal sealed class ChangePasswordHandler(
    ICurrentUserService currentUserService,
    UserManager<ApplicationUser> userManager)
    : IRequestHandler<ChangePasswordRequest>
{
    public async ValueTask<Result> Handle(
        ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(currentUserService.Id);

        if (user is null)
            return Error.InternalServer("Não foi possível identificar o usuário autenticado.");

        var result = await userManager
            .ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        return MapIdentityResult(result);
    }

    private static Result MapIdentityResult(IdentityResult result)
    {
        if (result.Succeeded)
            return Result.Ok();

        var isWrongPassword = result.Errors
            .Any(e => e.Code == "PasswordMismatch");

        return isWrongPassword
            ? Error.Unauthorized("Senha atual incorreta.")
            : Error.InternalServer("Não foi possível alterar a senha.");
    }
}