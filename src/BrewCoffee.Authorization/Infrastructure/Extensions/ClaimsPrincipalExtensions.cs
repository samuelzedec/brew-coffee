using System.Security.Claims;
using OpenIddict.Abstractions;

namespace BrewCoffee.Authorization.Infrastructure.Extensions;

public static class ClaimsPrincipalExtensions
{
    extension(ClaimsPrincipal principal)
    {
        /// <summary>
        /// Configura as informações do <see cref="ClaimsPrincipal"/> com base em
        /// identificadores e escopos fornecidos.
        /// </summary>
        /// <param name="scopes">
        /// Uma coleção de escopos que serão configurados no <see cref="ClaimsPrincipal"/>.
        /// Pode ser nulo caso não haja escopos específicos a serem utilizados.
        /// </param>
        public void ConfigurePrincipal(IEnumerable<string>? scopes = null)
        {
            var subject =
                principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? principal.FindFirst("sub")?.Value;

            if (subject is not null)
                principal.SetClaim(OpenIddictConstants.Claims.Subject, subject);

            principal.SetScopes(scopes);

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