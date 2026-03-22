using ZedEndpoints.Abstractions;

namespace BrewCoffee.Authorization.Features.Connect.ExternalCallback;

internal sealed class ExternalCallbackEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/external-callback", HandleAsync).ExcludeFromDescription();

    private static IResult HandleAsync(HttpContext context)
        => Results.Redirect(context.Request.Query["returnUrl"].FirstOrDefault() ?? "/connect/authorize");
}