using System.Security.Claims;
using BrewCoffee.Shared.Abstractions.Services;

namespace BrewCoffee.Authorization.Infrastructure.Services;

internal sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor)
    : ICurrentUserService
{
    private ClaimsPrincipal User => httpContextAccessor.HttpContext!.User;

    /// <inheritdoc/>
    public string Id => User.FindFirstValue("sub")!;

    /// <inheritdoc/>
    public string Email => User.FindFirstValue("email")!;

    /// <inheritdoc/>
    public bool IsAuthenticated => User.Identity?.IsAuthenticated ?? false;
}