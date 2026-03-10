using BrewCoffee.Shared.Common;

namespace BrewCoffee.Shared.Abstractions.Request;

/// <summary>
/// Representa um manipulador responsável por processar uma requisição específica, permitindo a execução
/// da lógica necessária para tratar a solicitação e retornar um resultado.
/// </summary>
/// <typeparam name="TRequest">
/// O tipo da requisição que será processada. Deve implementar a interface <see cref="IRequest"/>.
/// </typeparam>
public interface IRequestHandler<in TRequest> : Mediator.IRequestHandler<TRequest, Result>
    where TRequest : IRequest;

/// <summary>
/// Define um manipulador de requisições que utiliza o MediatR e padroniza
/// as respostas da aplicação utilizando <see cref="Result&lt;TResponse&gt;"/>.
/// </summary>
/// <typeparam name="TRequest">
/// Tipo da requisição que será processada.
/// </typeparam>
/// <typeparam name="TResponse">
/// Tipo do dado retornado quando a operação for bem-sucedida.
/// </typeparam>
public interface IRequestHandler<in TRequest, TResponse> : Mediator.IRequestHandler<TRequest, Result<TResponse>>
    where TRequest : IRequest<TResponse>;