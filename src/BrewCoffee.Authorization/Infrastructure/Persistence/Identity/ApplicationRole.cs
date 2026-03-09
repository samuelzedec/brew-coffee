using Microsoft.AspNetCore.Identity;

namespace BrewCoffee.Authorization.Infrastructure.Persistence.Identity;

internal sealed class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole()
    {
        Id = Guid.CreateVersion7();
    }
}