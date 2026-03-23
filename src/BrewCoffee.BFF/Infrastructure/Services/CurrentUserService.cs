using System.Security.Claims;
using BrewCoffee.Shared.Abstractions.Services;

namespace BrewCoffee.BFF.Infrastructure.Services;

internal sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor)
    : ICurrentUserService
{
    private ClaimsPrincipal User => httpContextAccessor.HttpContext!.User;
    public string Id => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!;
    public string Name => User.FindFirstValue("name")!;
    public string Email => User.FindFirstValue("email")!;
    public bool IsAuthenticated => User.Identity?.IsAuthenticated ?? false;
}