// using BrewCoffee.Api.Infrastructure.Persistence;
// using Microsoft.AspNetCore.Hosting;
// using Microsoft.AspNetCore.Mvc.Testing;
// using Microsoft.AspNetCore.TestHost;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.DependencyInjection.Extensions;
//
// namespace CoffeeAgent.AcceptanceTests.Infrastructure;
//
// public sealed class CustomWebApplicationFactory
//     : WebApplicationFactory<Program>
// {
//     public required string ConnectionString { get; init; }
//
//     protected override void ConfigureWebHost(IWebHostBuilder builder)
//         => builder.ConfigureTestServices(ConfigureDatabase);
//
//     private void ConfigureDatabase(IServiceCollection services)
//     {
//         services.RemoveAll<DbContextOptions<CoffeeDbContext>>();
//         services.RemoveAll<CoffeeDbContext>();
//
//         services.AddDbContext<CoffeeDbContext>(options =>
//             options.UseNpgsql(ConnectionString));
//     }
// }