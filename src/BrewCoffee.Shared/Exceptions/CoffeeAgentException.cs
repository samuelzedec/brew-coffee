namespace BrewCoffee.Shared.Exceptions;

/// <summary>
/// Representa uma exceção personalizada utilizada pelo CoffeeAgent.
/// </summary>
public abstract class CoffeeAgentException(string message)
    : Exception(message);