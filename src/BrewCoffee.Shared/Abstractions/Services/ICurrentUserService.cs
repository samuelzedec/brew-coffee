namespace BrewCoffee.Shared.Abstractions.Services;

/// <summary>
/// Define um contrato para representar o usuário atualmente autenticado no sistema.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Identificador único do usuário atual.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Nome do usuário atualmente autenticado.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Endereço de e-mail associado ao usuário atual.
    /// </summary>
    string Email { get; }

    /// <summary>
    /// Indica se o usuário atual está autenticado.
    /// </summary>
    bool IsAuthenticated { get; }
}