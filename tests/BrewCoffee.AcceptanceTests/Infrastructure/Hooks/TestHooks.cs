// using CoffeeAgent.AcceptanceTests.Infrastructure.Fixtures;
// using Microsoft.AspNetCore.Mvc.Testing;
// using Microsoft.Extensions.DependencyInjection;
// using Reqnroll;
// using Reqnroll.Microsoft.Extensions.DependencyInjection;
//
// namespace CoffeeAgent.AcceptanceTests.Infrastructure.Hooks;
//
// [Binding]
// public sealed class TestHooks
// {
//     private static DatabaseFixture s_dbFixture = null!;
//     private static CustomWebApplicationFactory s_factory = null!;
//
//     [BeforeTestRun]
//     public static async Task BeforeTestRun()
//     {
//         s_dbFixture = new DatabaseFixture();
//         await s_dbFixture.InitializeAsync();
//         s_factory = new CustomWebApplicationFactory { ConnectionString = s_dbFixture.ConnectionString };
//
//         await WithDbContextAsync(db => db.Database.MigrateAsync());
//     }
//
//     [AfterTestRun]
//     public static async Task AfterTestRun()
//     {
//         await s_factory.DisposeAsync();
//         await s_dbFixture.DisposeAsync();
//     }
//
//     [BeforeScenario]
//     public static async Task BeforeScenario()
//         => await WithDbContextAsync(DatabaseFixture.ResetDatabaseAsync);
//
//     [ScenarioDependencies]
//     public static IServiceCollection CreateServices()
//     {
//         var services = new ServiceCollection();
//         services.AddSingleton(s_factory);
//         services.AddSingleton(s_factory.Services);
//         services.AddScoped(_ => s_factory
//             .CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false }));
//
//         return services;
//     }
//
//     private static async Task WithDbContextAsync(Func<CoffeeDbContext, Task> action)
//     {
//         using var scope = s_factory.Services.CreateScope();
//         var db = scope.ServiceProvider.GetRequiredService<CoffeeDbContext>();
//         await action(db);
//     }
// }