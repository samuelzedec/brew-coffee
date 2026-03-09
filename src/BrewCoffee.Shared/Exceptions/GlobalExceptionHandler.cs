using System.Text.Json;
using System.Text.Json.Serialization;
using BrewCoffee.Shared.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BrewCoffee.Shared.Exceptions;

/// <summary>
/// Define uma classe responsável por centralizar o tratamento de exceções
/// globais em uma aplicação ASP.NET Core, utilizando um middleware personalizado.
/// </summary>
public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    private const string InternalServerErrorMessage 
        = "Ocorreu um erro inesperado. Tente novamente mais tarde.";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Exception occurred: {ExceptionType} - {Message}",
            exception.GetType().Name, exception.Message);

        (ErrorType errorType, string message, Dictionary<string, string[]>? errors) = GetErrorDetails(exception);
        await WriteErrorResponseAsync(httpContext, errorType, message, errors, cancellationToken);
        return true;
    }

    private static (ErrorType, string, Dictionary<string, string[]>?) GetErrorDetails(Exception exception)
        => exception switch
        {
            CoffeeAgentException ex => (ErrorType.UnprocessableEntity, ex.Message, null),
            CoffeeValidationException ex => (ErrorType.BadRequest, exception.Message, ex.Errors),
            _ => (ErrorType.InternalServerError, InternalServerErrorMessage, null)
        };

    private static async Task WriteErrorResponseAsync(
        HttpContext httpContext,
        ErrorType errorType,
        string message,
        Dictionary<string, string[]>? errors = null,
        CancellationToken cancellationToken = default)
    {
        var response = new Error(errorType, message, errors);
        httpContext.Response.StatusCode = (int)errorType;
        httpContext.Response.ContentType = "application/json;";

        await JsonSerializer.SerializeAsync(
            httpContext.Response.Body,
            response,
            JsonOptions,
            cancellationToken
        );
    }
}