using BrewCoffee.Authorization.Infrastructure.Setups;

var builder = WebApplication.CreateBuilder(args);
builder.Configure();

var app = builder.Build();
await app.Configure();

await app.RunAsync();