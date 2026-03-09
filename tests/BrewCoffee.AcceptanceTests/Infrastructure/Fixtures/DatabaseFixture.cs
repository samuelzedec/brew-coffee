// using CoffeeAgent.Api.Infrastructure.Persistence;
// using Microsoft.EntityFrameworkCore;
// using Testcontainers.PostgreSql;
// using Xunit;
//
// namespace CoffeeAgent.AcceptanceTests.Infrastructure.Fixtures;
//
// public sealed class DatabaseFixture : IAsyncLifetime
// {
//     private PostgreSqlContainer _dbContainer = null!;
//     public string ConnectionString { get; private set; } = null!;
//
//     public async ValueTask InitializeAsync()
//     {
//         _dbContainer = new PostgreSqlBuilder("postgres:18-alpine")
//             .WithDatabase("coffee-agent-test")
//             .WithUsername("postgres")
//             .WithPassword("root")
//             .Build();
//
//         await _dbContainer.StartAsync();
//         ConnectionString = _dbContainer.GetConnectionString();
//     }
//
//     public async ValueTask DisposeAsync()
//         => await _dbContainer.DisposeAsync();
//
//     internal static async Task ResetDatabaseAsync(CoffeeDbContext context)
//     {
//         var tableNames = await context.Database
//             .SqlQueryRaw<string>(
//                 """
//                     SELECT tablename 
//                     FROM pg_tables 
//                     WHERE schemaname='public' 
//                     AND tablename != '__EFMigrationsHistory';
//                 """)
//             .ToListAsync();
//
// #pragma warning disable EF1002
//         foreach (string tableName in tableNames)
//             await context.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE \"{tableName}\" RESTART IDENTITY CASCADE;");
// #pragma warning restore EF1002
//     }