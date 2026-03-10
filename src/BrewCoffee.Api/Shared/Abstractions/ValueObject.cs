namespace BrewCoffee.Api.Shared.Abstractions;

/// <summary>
/// Classe abstrata que serve como base para a implementação de objetos de valor.
/// Objetos de valor são caracterizados por sua identidade baseada em seus
/// atributos ou propriedades, sendo imutáveis e garantindo igualdade estrutural.
/// </summary>
public abstract record ValueObject
{
    /// <summary>
    /// Normaliza uma string, removendo espaços em branco no início e no final
    /// da string e convertendo todos os caracteres para minúsculas.
    /// </summary>
    /// <param name="value">A string que será normalizada.</param>
    /// <returns>Uma nova string normalizada, sem espaços em branco nas extremidades
    /// e com todos os caracteres em minúsculas.</returns>
    protected static string ToNormalized(string value)
        => value.Trim().ToLower();
}