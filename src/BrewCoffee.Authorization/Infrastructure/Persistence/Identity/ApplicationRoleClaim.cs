using Microsoft.AspNetCore.Identity;

namespace BrewCoffee.Authorization.Infrastructure.Persistence.Identity;

internal sealed class ApplicationRoleClaim : IdentityRoleClaim<Guid>;