using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Common
{
    /// <summary>
    /// Cuando un dato no se puede leer (texto en un campo numérico, un valor que no existe en la lista, un JSON
    /// mal formado), responde con <see cref="ErrorResponse"/> en español en vez del ProblemDetails de ASP.NET.
    /// </summary>
    internal static class InvalidModelStateResponse
    {
        public static IActionResult Create(ActionContext context)
        {
            // Los campos del JSON vienen como "$.salePrice"; las demás claves (el nombre interno del parámetro) no le
            // dicen nada al usuario.
            var fields = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0 && e.Key.StartsWith("$."))
                .Select(e => e.Key[2..])
                .Distinct()
                .ToArray();

            var message = fields.Length == 0
                ? "Los datos enviados no tienen el formato esperado."
                : $"Algunos datos no tienen el formato esperado ({string.Join(", ", fields)}). Revisa e inténtalo de nuevo.";

            return new BadRequestObjectResult(ErrorResponse.From([message]));
        }
    }
}
