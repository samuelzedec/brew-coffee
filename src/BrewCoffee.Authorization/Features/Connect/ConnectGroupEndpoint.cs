using BrewCoffee.Authorization.Features.Connect.Authorize;
using BrewCoffee.Authorization.Features.Connect.EndSession;
using BrewCoffee.Authorization.Features.Connect.ExternalCallback;
using BrewCoffee.Authorization.Features.Connect.ExternalLogin;
using BrewCoffee.Authorization.Features.Connect.Token;
using BrewCoffee.Authorization.Features.Connect.UserInfo;
using ZedEndpoints.Abstractions;
using ZedEndpoints.Attributes;
using ZedEndpoints.Extensions;

namespace BrewCoffee.Authorization.Features.Connect;

[NoGlobalPrefix]
internal sealed class ConnectGroupEndpoint : IEndpointGroup
{
    public void MapGroup(IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/connect")
            .WithTags("Connect");

        group
            .MapEndpoint<AuthorizeEndpoint>()
            .MapEndpoint<ExternalCallbackEndpoint>()
            .MapEndpoint<TokenEndpoint>()
            .MapEndpoint<UserInfoEndpoint>()
            .MapEndpoint<EndSessionEndpoint>()
            .MapEndpoint<ExternalLoginEndpoint>();
    }
}