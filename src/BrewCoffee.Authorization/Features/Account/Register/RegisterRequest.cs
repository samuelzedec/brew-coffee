using BrewCoffee.Shared.Abstractions.Request;

namespace BrewCoffee.Authorization.Features.Account.Register;

internal sealed record RegisterRequest(
    string UserName,
    string Email,
    string Password
) : IRequest<RegisterResponse>;

internal sealed record RegisterResponse(
    Guid Id,
    string Email
);