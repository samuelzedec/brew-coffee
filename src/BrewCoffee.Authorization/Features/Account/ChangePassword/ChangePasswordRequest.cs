using BrewCoffee.Shared.Abstractions.Request;

namespace BrewCoffee.Authorization.Features.Account.ChangePassword;

internal sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
) : IRequest;