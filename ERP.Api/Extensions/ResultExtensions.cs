using ERP.Api.Common;
using ERP.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Extensions
{
    public static class ResultExtensions
    {
        extension<T>(Result<T> result)
        {
            public IActionResult ToActionResult(int successStatusCode) =>
                result.IsSuccess
                ? new ObjectResult(result.Value) { StatusCode = successStatusCode }
                : Failure(result.Details, result.ErrorType);
        }

        extension(Result result)
        {
            public IActionResult ToActionResult(int successStatusCode) =>
                result.IsSuccess
                ? new StatusCodeResult(successStatusCode)
                : Failure(result.Details, result.ErrorType);
        }

        private static IActionResult Failure(IReadOnlyCollection<ErrorDetail> errors, ErrorType? errorType)
        {
            var statusCode = errorType switch
            {
                ErrorType.BadRequest => StatusCodes.Status400BadRequest,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Unavailable => StatusCodes.Status503ServiceUnavailable,
                ErrorType.TooManyRequests => StatusCodes.Status429TooManyRequests,
                _ => StatusCodes.Status500InternalServerError
            };

            return new ObjectResult(new ErrorResponse(errors)) { StatusCode = statusCode };
        }
    }
}
