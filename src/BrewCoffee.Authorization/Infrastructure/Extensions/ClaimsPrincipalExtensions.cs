using System.Security.Claims;
using OpenIddict.Abstractions;

namespace BrewCoffee.Authorization.Infrastructure.Extensions;

public static class ClaimsPrincipalExtensions
{
    extension(ClaimsPrincipal principal)
    {
        /// <summary>
        /// Configura um objeto <see cref="ClaimsPrincipal"/> com escopos padrão e destinos de claims.
        /// </summary>
        /// <remarks>
        /// Este método define escopos padrão no principal, como OpenId, Email, Profile e OfflineAccess.
        /// Além disso, associa destinos específicos às claims com base em seu tipo.
        /// </remarks>
        public void ConfigurePrincipal()
        {
            principal.SetScopes([
                OpenIddictConstants.Scopes.OpenId,
                OpenIddictConstants.Scopes.Email,
                OpenIddictConstants.Scopes.Profile,
                OpenIddictConstants.Scopes.OfflineAccess
            ]);

            principal.SetDestinations(claim => claim.Type switch
            {
                ClaimTypes.Name or ClaimTypes.Email =>
                [
                    OpenIddictConstants.Destinations.AccessToken,
                    OpenIddictConstants.Destinations.IdentityToken
                ],
                _ => [OpenIddictConstants.Destinations.AccessToken]
            });
        }
    }
}