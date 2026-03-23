using BrewCoffee.Authorization.Infrastructure.Persistence.Identity;
using BrewCoffee.Shared.Abstractions.Requests;
using BrewCoffee.Shared.Abstractions.Services;
using BrewCoffee.Shared.Common;
using Microsoft.AspNetCore.Identity;

namespace BrewCoffee.Authorization.Features.Account.Profile;

internal sealed class ProfileRequestHandler(
    UserManager<ApplicationUser> userManager,
    ICurrentUserService currentUserService)
    : IRequestHandler<ProfileRequest>
{
    public async ValueTask<Result> Handle(
        ProfileRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(currentUserService.Id);
        if (user is null) 
            return Error.NotFound("Usuário não encontrado.");

        var userNameResult = await EnsureUniqueUserNameAsync(request.NewUsername, user);
        if (userNameResult.IsFailure) 
            return userNameResult;

        var emailResult = await EnsureUniqueEmailAsync(request.NewEmail, user);
        if (emailResult.IsFailure) 
            return emailResult;

        var updateResult = await userManager.UpdateAsync(user);
        return updateResult.Succeeded
            ? Result.Ok()
            : Error.InternalServer("Não foi possível atualizar o perfil.");
    }

    private async ValueTask<Result> EnsureUniqueUserNameAsync(
        string? newUserName,
        ApplicationUser user)
    {
        if (string.IsNullOrEmpty(newUserName))
            return Result.Ok();

        if (await userManager.FindByNameAsync(newUserName) is not null)
            return Error.Conflict("Nome de usuário já está em uso.");

        user.UserName = newUserName;
        return Result.Ok();
    }

    private async ValueTask<Result> EnsureUniqueEmailAsync(
        string? newEmail,
        ApplicationUser user)
    {
        if (string.IsNullOrEmpty(newEmail))
            return Result.Ok();

        if (await userManager.FindByNameAsync(newEmail) is not null)
            return Error.Conflict("Email já está em uso.");

        if (!await userManager.HasPasswordAsync(user))
            return Error.BadRequest("É necessário ter uma senha cadastrada para alterar o email.");

        await RemoveProvidersAsync(user);
        user.Email = newEmail;
        return Result.Ok();
    }

    private async Task RemoveProvidersAsync(ApplicationUser user)
    {
        var loginsInfo = await userManager.GetLoginsAsync(user);
        foreach (var loginInfo in loginsInfo)
            await userManager.RemoveLoginAsync(user, loginInfo.LoginProvider, loginInfo.ProviderKey);
    }
}