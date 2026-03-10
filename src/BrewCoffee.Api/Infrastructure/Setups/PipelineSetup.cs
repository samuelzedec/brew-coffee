using BrewCoffee.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using ZedEndpoints.Extensions;

namespace BrewCoffee.Api.Infrastructure.Setups;

internal static class PipelineSetup
{
    extension(WebApplication app)
    {
        /// <summary>
        /// Configura os componentes essenciais da aplicação, incluindo redirecionamento HTTPS,
        /// documentação da API, agrupamento de endpoints e o manipulador de exceções.
        /// Este método é responsável por estabelecer os pipelines básicos de execução do aplicativo.
        /// </summary>
        public async Task ConfigureAsync()
        {
            if (!app.Environment.IsDevelopment())
                app.UseHttpsRedirection();

            app.ConfigureApiDocumentation();
            app.MapEndpointGroups(globalPrefix: "api/v1");
            app.UseExceptionHandler();
            await app.ApplyMigrationsAsync();
        }

        private async Task ApplyMigrationsAsync()
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<BrewCoffeeDbContext>();
            await context.Database.MigrateAsync();
        }

        private void ConfigureApiDocumentation()
        {
            if (!app.Environment.IsDevelopment())
                return;

            app.MapOpenApi();
            app.MapScalarApiReference();
        }
    }
}