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

            app.UseExceptionHandler();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapReverseProxy();
            app.MapEndpointGroups();
        }
    }
}