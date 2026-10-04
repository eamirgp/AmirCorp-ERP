using ERP.Application.Common.Results;

namespace ERP.Api.Common
{
    /// <summary>
    /// La forma de toda respuesta de error: <c>{ "errors": [{ "message": "...", "field": "code" }] }</c>. <c>field</c> dice
    /// de qué campo del pedido es el error (la pantalla lo pone debajo de ese campo); si es null, es del pedido entero
    /// (decisión 37).
    /// </summary>
    public sealed record ErrorResponse(
        IReadOnlyCollection<ErrorDetail> Errors
        )
    {
        /// <summary>Errores sin campo: del pedido entero.</summary>
        public static ErrorResponse From(IEnumerable<string> messages) =>
            new(messages.Select(m => new ErrorDetail(m)).ToArray());
    }
}
