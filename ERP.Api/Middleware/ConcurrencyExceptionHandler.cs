using ERP.Api.Common;
using ERP.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace ERP.Api.Middleware
{
    internal sealed class ConcurrencyExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is not ConcurrencyException concurrencyException)
                return false;

            httpContext.Response.StatusCode = StatusCodes.Status409Conflict;

            await httpContext.Response.WriteAsJsonAsync(
                ErrorResponse.From([concurrencyException.Message]),
                cancellationToken
                );

            return true;
        }
    }
}
