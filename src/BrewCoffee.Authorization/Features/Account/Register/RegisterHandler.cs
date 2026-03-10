using BrewCoffee.Authorization.Infrastructure.Persistence.Identity;
using BrewCoffee.Shared.Abstractions.Request;
using BrewCoffee.Shared.Common;
using Microsoft.AspNetCore.Identity;

namespace BrewCoffee.Authorization.Features.Account.Register;

internal sealed class RegisterHandler(
    UserManager<ApplicationUser> userManager,
    ILogger<RegisterHandler> logger)
    : IRequestHandler<RegisterRequest, RegisterResponse>
{
    public async ValueTask<Result<RegisterResponse>> Handle(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var validateRequest = await CheckConflictsAsync(request);

        if (validateRequest.IsFailure)
            return Error.BadRequest(validateRequest.Error);

        var applicationUser = new ApplicationUser { UserName = request.UserName, Email = request.Email };
        var createdResult = await userManager.CreateAsync(applicationUser, request.Password);

        if (!createdResult.Succeeded)
            return LogIdentityErrors(createdResult);

        return new RegisterResponse(applicationUser.Id, applicationUser.Email);
    }

    private async Task<Result> CheckConflictsAsync(RegisterRequest request)
    {
        if (await userManager.FindByEmailAsync(request.Email) is not null)
            return Error.Conflict("Usuário com e-mail já cadastrado");

        if (await userManager.FindByNameAsync(request.UserName) is not null)
            return Error.Conflict("Usuário com nome já cadastrado.");

        return Result.Ok();
    }

    private Error LogIdentityErrors(IdentityResult result)
    {
        var errors = result.Errors.Select(x => x.Description);
        logger.LogError(
            "Identity errors while creating user: {Errors}",
            string.Join(", ", errors)
        );

        return Error.InternalServer("Não foi possível criar o usuário. Tente novamente mais tarde!");
    }
}