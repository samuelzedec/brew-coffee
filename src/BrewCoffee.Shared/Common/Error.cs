using System.Text.Json.Serialization;

namespace BrewCoffee.Shared.Common;

public sealed class Error
{
    public string Message { get; init; } = string.Empty;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public ErrorType? Type { get; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public Dictionary<string, string[]>? Details { get; init; }

    [JsonConstructor]
    public Error() { }

    public Error(ErrorType type, string message, Dictionary<string, string[]>? details = null)
    {
        Type = type;
        Message = message;
        Details = details;
    }

    public static Error NotFound(string message)
        => new(ErrorType.NotFound, message);

    public static Error BadRequest(string message)
        => new(ErrorType.BadRequest, message);

    public static Error Conflict(string message)
        => new(ErrorType.Conflict, message);

    public static Error Unauthorized(string message)
        => new(ErrorType.Unauthorized, message);

    public static Error Forbidden(string message)
        => new(ErrorType.Forbidden, message);

    public static Error InternalServer(string message)
        => new(ErrorType.InternalServerError, message);

    public static implicit operator string(Error error)
        => error.Message;
}