using Microsoft.AspNetCore.Identity;

namespace BrewCoffee.Authorization.Infrastructure.Persistence.Identity;

internal sealed class ApplicationUser : IdentityUser<Guid>
{
    public ApplicationUser()
    {
        Id = Guid.CreateVersion7();
    }
}