using Microsoft.EntityFrameworkCore;

namespace BrewCoffee.Api.Infrastructure.Persistence;

internal sealed class BrewCoffeeDbContext(DbContextOptions<BrewCoffeeDbContext> options) 
    : DbContext(options)
{
    
}