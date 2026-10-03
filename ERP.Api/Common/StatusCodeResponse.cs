using Microsoft.AspNetCore.Diagnostics;

namespace ERP.Api.Common
{
    /// <summary>
    /// Respuestas de error que salen sin cuerpo (sesión vencida, sin permiso, dirección que no existe, método no
    /// permitido): les pone la forma de siempre, <c>{ errors: [...] }</c>, con un mensaje en español (decisión 10). Las
    /// que ya traen su mensaje (un 404 de un caso de uso, el 401 con el motivo de la sesión) no se tocan.
    /// </summary>
    internal static class StatusCodeResponse
    {
        public static async Task WriteAsync(StatusCodeContext context)
        {
            var response = context.HttpContext.Response;
            var message = response.StatusCode switch
            {
                StatusCodes.Status401Unauthorized => "Tu sesión terminó o no has iniciado sesión. Vuelve a iniciar sesión.",
                StatusCodes.Status403Forbidden => "No tienes permiso para hacer esto. Si lo necesitas, pídeselo a un administrador.",
                StatusCodes.Status404NotFound => "La dirección pedida no existe en la API.",
                StatusCodes.Status405MethodNotAllowed => "Esa acción no se puede hacer en esta dirección de la API.",
                StatusCodes.Status413PayloadTooLarge => "El archivo es demasiado grande. Divídelo en varios archivos más pequeños.",
                StatusCodes.Status415UnsupportedMediaType => "El pedido debe enviarse en formato JSON.",
                StatusCodes.Status429TooManyRequests => "Hiciste demasiadas consultas seguidas. Espera un momento e inténtalo de nuevo.",
                _ => "No se pudo completar el pedido."
            };

            await response.WriteAsJsonAsync(new ErrorResponse([message]));
        }
    }
}
