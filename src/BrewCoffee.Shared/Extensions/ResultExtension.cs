using BrewCoffee.Shared.Common;
using Microsoft.AspNetCore.Http;

namespace BrewCoffee.Shared.Extensions;

public static class ResultExtension
{
    extension(Result result)
    {
        public IResult ToActionResult() => result.IsSuccess
            ? Results.NoContent()
            : Results.Json(
                result.Error,
                statusCode: (int)result.Error.Type!.Value
            );
    }

    extension<T>(Result<T> result)
    {
        public IResult ToActionResult(string? routeName = null)
        {
            if (result.IsSuccess && routeName is not null)
                return Results.CreatedAtRoute(routeName, null, result.Value);

            if (result.IsSuccess)
                return Results.Ok(result.Value);

            return Results.Json(
                result.Error,
                statusCode: (int)result.Error.Type!.Value
            );
        }
    }
}