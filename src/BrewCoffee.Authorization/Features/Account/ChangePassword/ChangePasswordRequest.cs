using BrewCoffee.Shared.Abstractions.Requests;

namespace BrewCoffee.Authorization.Features.Account.ChangePassword;

internal sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
) : IRequest;