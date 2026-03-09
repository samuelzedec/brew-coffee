namespace BrewCoffee.Authorization.Infrastructure.Settings;

/// <summary>
/// Representa as configurações de autenticação para um cliente específico.
/// </summary>
/// <remarks>
/// A classe contém as informações de identificação e segredo necessárias para realizar autenticação.
/// </remarks>
/// <param name="Id">
/// O identificador único do cliente.
/// </param>
/// <param name="Secret">
/// O segredo associado ao cliente, usado para autenticação.
/// </param>
internal sealed record ClientSettings(
    string Id,
    string Secret
);