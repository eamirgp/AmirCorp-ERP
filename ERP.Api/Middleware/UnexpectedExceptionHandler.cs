using ERP.Api.Common;
using Microsoft.AspNetCore.Diagnostics;

namespace ERP.Api.Middleware
{
    /// <summary>
    /// Último recurso: cualquier error no previsto responde 500 con la misma forma que el resto
    /// (<see cref="ErrorResponse"/>), así la pantalla siempre puede mostrar un mensaje en español.
    /// El detalle técnico queda en el log, no se envía al usuario.
    /// </summary>
    internal sealed class UnexpectedExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<UnexpectedExceptionHandler> _logger;

        public UnexpectedExceptionHandler(ILogger<UnexpectedExceptionHandler> logger) => _logger = logger;

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            // El navegador cerró el pedido (cambió de página, cerró la pestaña): no es un error y no hay a quién responder.
            if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
            {
                _logger.LogInformation("Pedido cancelado por el cliente: {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
                httpContext.Response.StatusCode = 499;
                return true;
            }

            _logger.LogError(exception, "Error no controlado en {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);

            // Si la respuesta ya empezó a enviarse (por ejemplo, un archivo), no se puede cambiar: queda solo en el log.
            if (httpContext.Response.HasStarted)
                return true;

            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await httpContext.Response.WriteAsJsonAsync(
                new ErrorResponse(["Ocurrió un error inesperado. Vuelve a intentarlo; si continúa, avisa al administrador."]),
                cancellationToken
                );

            return true;
        }
    }
}
