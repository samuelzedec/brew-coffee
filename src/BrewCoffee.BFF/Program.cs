using BrewCoffee.BFF.Infrastructure.Setups;

var builder = WebApplication.CreateBuilder(args);
builder.Configure();

var app = builder.Build();
app.ConfigurePipeline();

await app.RunAsync();