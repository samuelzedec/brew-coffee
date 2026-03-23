using BrewCoffee.Authorization.Infrastructure.Persistence.Identity;
using BrewCoffee.Shared.Abstractions.Requests;
using BrewCoffee.Shared.Abstractions.Services;
using BrewCoffee.Shared.Common;
using Microsoft.AspNetCore.Identity;

namespace BrewCoffee.Authorization.Features.Account.ChangePassword;

internal sealed class ChangePasswordRequestHandler(
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

        (IdentityResult result, bool hasPassword) = await EnsurePasswordAsync(user, request);
        return MapIdentityResult(result, hasPassword);
    }

    private async Task<(IdentityResult, bool)> EnsurePasswordAsync(
        ApplicationUser user,
        ChangePasswordRequest request)
        => await userManager.HasPasswordAsync(user)
            ? (await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword), true)
            : (await userManager.AddPasswordAsync(user, request.NewPassword), false);

    private static Result MapIdentityResult(
        IdentityResult result,
        bool hasPassword)
    {
        if (result.Succeeded)
            return Result.Ok();

        var isWrongPassword = result.Errors
            .Any(e => e.Code == "PasswordMismatch");

        return isWrongPassword && hasPassword
            ? Error.Unauthorized("Senha atual incorreta.")
            : Error.InternalServer("Não foi possível alterar a senha.");
    }
}