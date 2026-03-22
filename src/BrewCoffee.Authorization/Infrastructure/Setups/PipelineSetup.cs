using BrewCoffee.Authorization.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using ZedEndpoints.Extensions;

namespace BrewCoffee.Authorization.Infrastructure.Setups;

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

            app.MapEndpointGroups();
            app.UseExceptionHandler();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapRazorPages();
            app.MapGet("/", () => Results.Redirect("/login"));
            await app.ApplyMigrationsAsync();
        }

        private async Task ApplyMigrationsAsync()
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<BrewCoffeeAuthDbContext>();
            await context.Database.MigrateAsync();
        }
    }
}