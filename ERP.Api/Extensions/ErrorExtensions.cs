using ERP.Api.Common;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Extensions
{
    public static class ErrorExtensions
    {
        extension(IReadOnlyCollection<string> errors)
        {
            public IActionResult ToBadRequest() =>
                new ObjectResult(new ErrorResponse(errors)) { StatusCode = StatusCodes.Status400BadRequest };
        }
    }
}
