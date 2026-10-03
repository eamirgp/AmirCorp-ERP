using ERP.Api.Common;
using ERP.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;

namespace ERP.Api.Middleware
{
    /// <summary>
    /// Una regla del dominio que el caso de uso no revisó antes: se responde 400 con el mensaje, y queda un aviso en el
    /// registro porque es un hueco (decisión 27: los errores esperados se revisan antes y salen todos juntos).
    /// </summary>
    internal sealed class DomainExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<DomainExceptionHandler> _logger;

        public DomainExceptionHandler(ILogger<DomainExceptionHandler> logger) => _logger = logger;

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is not DomainException domainException)
                return false;

            _logger.LogWarning("Regla del dominio no revisada antes en {Method} {Path}: {Message}",
                httpContext.Request.Method, httpContext.Request.Path, domainException.Message);

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

            await httpContext.Response.WriteAsJsonAsync(
                new ErrorResponse([domainException.Message]),
                cancellationToken
                );

            return true;
        }
    }
}
