using BrewCoffee.Shared.Abstractions.Requests;

namespace BrewCoffee.Authorization.Features.Account.HasPassword;

internal sealed record HasPasswordRequest : IRequest<HasPasswordRequestResponse>;

internal sealed record HasPasswordRequestResponse(bool HasPassword);