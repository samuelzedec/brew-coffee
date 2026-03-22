using Scalar.AspNetCore;
using ZedEndpoints.Extensions;

namespace BrewCoffee.BFF.Infrastructure.Setups;

internal static class PipelineSetup
{
    extension(WebApplication app)
    {
        public void ConfigurePipeline()
        {
            if (!app.Environment.IsDevelopment())
                app.UseHttpsRedirection();

            app.ConfigureApiDocumentation();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapReverseProxy();
            app.MapEndpointGroups();
        }

        private void ConfigureApiDocumentation()
        {
            if (!app.Environment.IsDevelopment())
                return;

            app.MapOpenApi();
            app.MapScalarApiReference();
            app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
        }
    }
}