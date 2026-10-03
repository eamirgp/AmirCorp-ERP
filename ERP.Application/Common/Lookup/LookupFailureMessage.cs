using ERP.Application.Common.Results;
using ERP.Application.Contracts.Infrastructure;

namespace ERP.Application.Common.Lookup
{
    /// <summary>
    /// Qué decir cuando un servicio externo no respondió como se esperaba, igual para la consulta de RUC o DNI y la del
    /// tipo de cambio. El "no encontrado" lo dice cada consulta, porque depende de qué se buscó.
    /// </summary>
    internal static class LookupFailureMessage
    {
        /// <param name="source">Quién publica el dato: "SUNAT" o "RENIEC".</param>
        /// <param name="byHand">Lo que el usuario escribe a mano mientras tanto: "el nombre", "el tipo de cambio".</param>
        /// <returns>El mensaje y el tipo de error (503), o null si la falla es "no encontrado".</returns>
        public static (string Message, ErrorType Type)? For(LookupFailure failure, string source, string byHand) =>
            failure switch
            {
                LookupFailure.Unauthorized => (
                    $"El servicio de consulta rechazó la clave: puede haber vencido. Avisa al administrador del sistema y, mientras tanto, escribe {byHand} a mano.",
                    ErrorType.Unavailable),
                LookupFailure.QuotaExceeded => (
                    $"Se acabaron las consultas a {source} de este mes. Escribe {byHand} a mano; las consultas vuelven el próximo mes.",
                    ErrorType.Unavailable),
                LookupFailure.Unavailable => (
                    $"No se pudo consultar {source} en este momento. Inténtalo en unos minutos o escribe {byHand} a mano.",
                    ErrorType.Unavailable),
                _ => null
            };
    }
}
