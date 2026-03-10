using BrewCoffee.Api.Shared.Abstractions;

namespace BrewCoffee.Api.Shared.Repositories;

/// <summary>
/// Define um contrato genérico para operações de repositório, permitindo a interação
/// com entidades no contexto de armazenamento de dados.
/// </summary>
/// <typeparam name="TEntity">
/// O tipo da entidade gerenciada pelo repositório. Deve ser uma derivada da classe <see cref="Entity"/>.
/// </typeparam>
/// <remarks>
/// Esta interface fornece métodos para criar, atualizar e excluir entidades, abstraindo a lógica
/// de acesso aos dados para as implementações específicas.
/// </remarks>
public interface IRepository<TEntity> where TEntity : Entity
{
    /// <summary>
    /// Cria uma nova entidade no repositório de dados de forma assíncrona.
    /// </summary>
    /// <param name="entity">
    /// A entidade a ser criada no repositório. Deve ser uma instância do tipo <typeparamref name="TEntity"/>,
    /// que herda da classe <see cref="Entity"/>.
    /// </param>
    /// <param name="cancellationToken">
    /// Um token de cancelamento opcional que pode ser usado para cancelar a operação assíncrona.
    /// </param>
    /// <returns>
    /// Uma <see cref="Task"/> que representa a operação assíncrona.
    /// </returns>
    Task CreateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza uma entidade existente no repositório de dados de forma assíncrona.
    /// </summary>
    /// <param name="entity">
    /// A entidade a ser atualizada no repositório. Deve ser uma instância do tipo <typeparamref name="TEntity"/>,
    /// que herda da classe <see cref="Entity"/>.
    /// </param>
    /// <param name="cancellationToken">
    /// Um token de cancelamento opcional que pode ser usado para cancelar a operação assíncrona.
    /// </param>
    /// <returns>
    /// Uma <see cref="Task"/> que representa a operação assíncrona.
    /// </returns>
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exclui uma entidade existente do repositório de dados de forma assíncrona.
    /// </summary>
    /// <param name="entity">
    /// A entidade a ser excluída do repositório. Deve ser uma instância do tipo <typeparamref name="TEntity"/>,
    /// que herda da classe <see cref="Entity"/>.
    /// </param>
    /// <param name="cancellationToken">
    /// Um token de cancelamento opcional que pode ser usado para cancelar a operação assíncrona.
    /// </param>
    /// <returns>
    /// Uma <see cref="Task"/> que representa a operação assíncrona.
    /// </returns>
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Recupera uma entidade do tipo <typeparamref name="TEntity"/> pelo seu identificador de maneira assíncrona.
    /// </summary>
    /// <param name="id">
    /// O identificador único da entidade a ser recuperada.
    /// </param>
    /// <param name="cancellationToken">
    /// Um token de cancelamento opcional que pode ser usado para cancelar a operação assíncrona.
    /// </param>
    /// <returns>
    /// Uma <see cref="Task"/> que representa a operação assíncrona. O valor retornado é a entidade do tipo
    /// <typeparamref name="TEntity"/> correspondente ao identificador fornecido, ou <c>null</c> caso ela não seja encontrada.
    /// </returns>
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}