using BrewCoffee.Shared.Common;

namespace BrewCoffee.Shared.Abstractions.Request;

/// <summary>
/// Representa uma solicitação que será manipulada no sistema.
/// </summary>
public interface IRequest : Mediator.IRequest<Result>;

/// <summary>
/// Define uma interface base para representar uma solicitação genérica no sistema.
/// </summary>
public interface IRequest<TRequestResponse> : Mediator.IRequest<Result<TRequestResponse>>;