namespace ERP.Application.Contracts.Infrastructure
{
    /// <summary>
    /// Consulta de un RUC en SUNAT a través de un proveedor externo (hoy Decolecta). Si no hay clave configurada,
    /// la consulta no está disponible y el formulario no muestra el botón: todo lo demás funciona igual.
    /// </summary>
    public interface IRucLookup
    {
        bool IsConfigured { get; }

        Task<RucLookupOutcome> FindAsync(string ruc, CancellationToken ct = default);
    }

    /// <summary>Datos del contribuyente tal como los informa SUNAT.</summary>
    public sealed record RucLookupData(
        string Ruc,
        string Name,
        // "ACTIVO", "BAJA DE OFICIO", "SUSPENSION TEMPORAL"…
        string Status,
        // "HABIDO", "NO HABIDO", "NO HALLADO"…
        string Condition,
        string? Address
        );

    public enum RucLookupFailure
    {
        /// <summary>SUNAT no tiene ese RUC.</summary>
        NotFound,
        /// <summary>El proveedor rechazó la clave (vencida o mal copiada).</summary>
        Unauthorized,
        /// <summary>El proveedor no respondió o falló.</summary>
        Unavailable
    }

    /// <summary>Resultado de la consulta: los datos o el motivo por el que no se obtuvieron.</summary>
    public sealed record RucLookupOutcome(RucLookupData? Data, RucLookupFailure? Failure)
    {
        public static RucLookupOutcome Found(RucLookupData data) => new(data, null);
        public static RucLookupOutcome Failed(RucLookupFailure failure) => new(null, failure);
    }
}
