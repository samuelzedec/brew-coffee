namespace BrewCoffee.Shared.Abstractions;

/// <summary>
/// Representa uma entidade base abstrata que fornece propriedades e funcionalidades comuns
/// para outros objetos dentro do sistema.
/// </summary>
/// <remarks>
/// Esta classe contém propriedades e métodos que gerenciam os atributos de identificação,
/// criação, atualização e exclusão de entidades no contexto da aplicação.
/// </remarks>
public abstract class Entity
{
    /// <summary>
    /// Obtém o identificador único da entidade.
    /// </summary>
    /// <remarks>
    /// O identificador é gerado de forma automática e imutável utilizando a versão 7 do UUID.
    /// Ele é utilizado para distinguir de forma única cada instância de entidade.
    /// </remarks>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>
    /// Obtém a data e hora em que a entidade foi criada.
    /// </summary>
    /// <remarks>
    /// Esta propriedade é definida automaticamente no momento da criação da entidade,
    /// utilizando o horário UTC. Ela é imutável, servindo como um registro temporal
    /// consistente para fins de auditoria e rastreamento.
    /// </remarks>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Obtém ou define a data e hora da última atualização da entidade.
    /// </summary>
    /// <remarks>
    /// Esta propriedade armazena o momento em que a entidade foi modificada pela última vez.
    /// Sua atualização ocorre automaticamente por meio do método <see cref="UpdateEntity"/>.
    /// Caso a entidade nunca tenha sido modificada, o valor será nulo.
    /// </remarks>
    public DateTimeOffset? UpdatedAt { get; private set; }

    /// <summary>
    /// Obtém a data e hora em que a entidade foi marcada como excluída.
    /// </summary>
    /// <remarks>
    /// Representa o momento em que a exclusão lógica da entidade foi realizada.
    /// Caso o valor seja nulo, significa que a entidade ainda não foi marcada como excluída.
    /// </remarks>
    public DateTimeOffset? DeletedAt { get; private set; }

    /// <summary>
    /// Atualiza a entidade, ajustando a propriedade que indica a data e hora da última modificação.
    /// </summary>
    /// <remarks>
    /// Este método é utilizado para registrar o momento exato em que a entidade foi alterada.
    /// Após a chamada deste método, a propriedade <c>UpdatedAt</c> será atualizada com o horário atual
    /// (em UTC) para refletir a última modificação.
    /// </remarks>
    public void UpdateEntity()
        => UpdatedAt = DateTimeOffset.UtcNow;

    /// <summary>
    /// Marca a entidade como excluída, definindo a propriedade que indica a data e hora da exclusão.
    /// </summary>
    /// <remarks>
    /// Este método é utilizado para registrar o momento exato em que a entidade foi excluída.
    /// Após a execução deste método, a propriedade <c>DeletedAt</c> será atualizada com o horário atual
    /// (em UTC) para refletir a exclusão da entidade.
    /// </remarks>
    public void DeleteEntity()
        => DeletedAt = DateTimeOffset.UtcNow;

    /// <summary>
    /// Retorna um código de hash que representa a entidade com base em sua propriedade <c>Id</c>.
    /// </summary>
    /// <remarks>
    /// Este método é utilizado para obter um identificador único de hash para a entidade,
    /// fundamentado no valor do identificador único (<c>Id</c>). É particularmente útil
    /// para inserir ou localizar instâncias em coleções que utilizam hashing, como <c>Dictionary</c>
    /// ou <c>HashSet</c>.
    /// </remarks>
    /// <returns>
    /// Um número inteiro que representa o código de hash da entidade gerado a partir do identificador único.
    /// </returns>
    public override int GetHashCode()
        => Id.GetHashCode();
}