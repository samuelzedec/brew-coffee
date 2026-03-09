using System.Net.Http.Json;
using System.Text.Json;
using BrewCoffee.Shared.Common;

namespace BrewCoffee.AcceptanceTests.Infrastructure.Extensions;

public static class HttpResponseExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    extension(HttpResponseMessage response)
    {
        /// <summary>
        /// Lê o conteúdo de uma resposta HTTP e tenta desserializá-lo em um objeto do tipo especificado.
        /// </summary>
        /// <typeparam name="T">
        /// O tipo do objeto para o qual o conteúdo da resposta será desserializado.
        /// </typeparam>
        /// <returns>
        /// Um objeto do tipo <typeparamref name="T"/>, contendo os dados desserializados,
        /// ou <c>null</c> caso o conteúdo da resposta esteja vazio ou não seja possível desserializá-lo.
        /// </returns>
        public async Task<T?> ReadAsAsync<T>()
            => await response.Content.ReadFromJsonAsync<T>(JsonOptions);

        /// <summary>
        /// Lê o conteúdo de uma resposta HTTP e tenta desserializá-lo em um objeto do tipo <see cref="Error"/>.
        /// </summary>
        /// <returns>
        /// Um objeto da classe <see cref="Error"/> contendo as informações do erro,
        /// ou <c>null</c> caso o conteúdo da resposta esteja vazio ou não seja possível desserializá-lo.
        /// </returns>
        public async Task<Error?> ReadErrorAsync()
            => await response.Content.ReadFromJsonAsync<Error>(JsonOptions);
    }
}