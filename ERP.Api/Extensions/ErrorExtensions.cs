using ERP.Api.Common;
using ERP.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Extensions
{
    public static class ErrorExtensions
    {
        extension(IReadOnlyCollection<string> errors)
        {
            public IActionResult ToBadRequest() =>
                new ObjectResult(ErrorResponse.From(errors)) { StatusCode = StatusCodes.Status400BadRequest };
        }

        /// <summary>Errores con su campo (decisión 37).</summary>
        extension(IReadOnlyCollection<ErrorDetail> errors)
        {
            public IActionResult ToBadRequest() =>
                new ObjectResult(new ErrorResponse(errors)) { StatusCode = StatusCodes.Status400BadRequest };
        }
    }
}
