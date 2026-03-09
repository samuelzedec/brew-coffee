namespace BrewCoffee.Shared.Exceptions;

/// <summary>
/// Representa uma exceção específica para falhas de validação relacionadas ao contexto do CoffeeAgent.
/// </summary>
public sealed class CoffeeValidationException(Dictionary<string, string[]> errors)
    : Exception("Dados inválidos.")
{
    public Dictionary<string, string[]> Errors => errors;
}