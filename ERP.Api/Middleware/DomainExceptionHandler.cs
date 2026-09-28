using ERP.Api.Common;
using ERP.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;

namespace ERP.Api.Middleware
{
    internal sealed class DomainExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is not DomainException domainException)
                return false;

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

            await httpContext.Response.WriteAsJsonAsync(
                new ErrorResponse([domainException.Message]),
                cancellationToken
                );

            return true;
        }
    }
}
