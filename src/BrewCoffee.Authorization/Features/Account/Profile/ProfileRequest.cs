using BrewCoffee.Shared.Abstractions.Requests;

namespace BrewCoffee.Authorization.Features.Account.Profile;

internal sealed record ProfileRequest(
    string? NewUsername,
    string? NewEmail
) : IRequest;