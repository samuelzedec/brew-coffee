// using BrewCoffee.Authorization.Features.User;
// using BrewCoffee.Authorization.Features.User.ValueObjects;
// using Microsoft.EntityFrameworkCore;
//
// namespace BrewCoffee.Authorization.Infrastructure.Persistence.Repositories;
//
// internal sealed class UserRepository(CoffeeAuthDbContext context)
//     : Repository<User>(context), IUserRepository
// {
//     public async Task<User?> FindByUsernameAsync(Username username, CancellationToken cancellationToken = default)
//         => await _table
//             .AsNoTracking()
//             .SingleOrDefaultAsync(u => u.Username.Value == username.Value, cancellationToken);
//
//     public async Task<User?> FindByEmailAsync(Email email, CancellationToken cancellationToken = default)
//         => await _table
//             .AsNoTracking()
//             .SingleOrDefaultAsync(u => u.Email.Value == email.Value, cancellationToken);
// }