using BrewCoffee.Authorization.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using ZedEndpoints.Extensions;

namespace BrewCoffee.Authorization.Infrastructure.Setups;

internal static class PipelineSetup
{
    extension(WebApplication app)
    {
        public async Task ConfigureAsync()
        {
            if (!app.Environment.IsDevelopment())
                app.UseHttpsRedirection();

            app.UseExceptionHandler();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapEndpointGroups();
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