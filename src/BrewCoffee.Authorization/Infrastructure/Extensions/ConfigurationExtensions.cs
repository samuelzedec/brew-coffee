using BrewCoffee.Authorization.Infrastructure.Settings;

namespace BrewCoffee.Authorization.Infrastructure.Extensions;

internal static class ConfigurationExtensions
{
    extension(IConfiguration configuration)
    {
        /// <summary>
        /// Recupera as configurações de autenticação para um provedor específico.
        /// </summary>
        /// <param name="provider">
        /// O nome do provedor para o qual as configurações de autenticação devem ser recuperadas.
        /// </param>
        /// <returns>
        /// Um objeto <see cref="ClientSettings"/> contendo as configurações de autenticação do provedor especificado.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando as configurações de `ClientId` ou `ClientSecret` para o provedor especificado não estão configuradas.
        /// </exception>
        public ClientSettings GetProviderAuth(string provider)
        {
            var section = configuration.GetSection($"Authentication:{provider}");
            var clientId = section["ClientId"];

            if (string.IsNullOrEmpty(clientId))
                throw new InvalidOperationException($"{provider} ClientId não configurado.");

            var clientSecret = section["ClientSecret"];

            if (string.IsNullOrEmpty(clientSecret))
                throw new InvalidOperationException($"{provider} ClientSecret não configurado.");

            return new ClientSettings(clientId, clientSecret);
        }

        /// <summary>
        /// Recupera a configuração específica de um cliente OpenIddict usando uma chave fornecida.
        /// </summary>
        /// <param name="client">
        /// O nome do cliente cujo valor de configuração deve ser recuperado.
        /// </param>
        /// <param name="key">
        /// A chave da configuração a ser recuperada para o cliente especificado.
        /// </param>
        /// <returns>
        /// O valor da configuração correspondente à chave fornecida para o cliente especificado.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando a configuração especificada para o cliente e a chave fornecidos não está configurada.
        /// </exception>
        public string GetOpenIddictClientConfig(string client, string key)
        {
            var value = configuration[$"OpenIddict:Clients:{client}:{key}"];

            if (string.IsNullOrEmpty(value))
                throw new InvalidOperationException($"OpenIddict client '{client}' {key} não configurado.");

            return value;
        }
    }
}