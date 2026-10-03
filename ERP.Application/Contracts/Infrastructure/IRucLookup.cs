namespace ERP.Application.Contracts.Infrastructure
{
    /// <summary>
    /// Consulta de un RUC en SUNAT o de un DNI en RENIEC a través de un proveedor externo (hoy Decolecta). Si no hay
    /// clave configurada, la consulta no está disponible y el formulario no muestra el botón: todo lo demás funciona igual.
    /// </summary>
    public interface IRucLookup
    {
        bool IsConfigured { get; }

        Task<RucLookupOutcome> FindAsync(string ruc, CancellationToken ct = default);

        Task<DniLookupOutcome> FindDniAsync(string dni, CancellationToken ct = default);
    }

    /// <summary>Resultado de la consulta de DNI: el nombre como lo informa RENIEC (apellidos y nombres) o el motivo del fallo.</summary>
    public sealed record DniLookupOutcome(string? Name, LookupFailure? Failure)
    {
        public static DniLookupOutcome Found(string name) => new(name, null);
        public static DniLookupOutcome Failed(LookupFailure failure) => new(null, failure);
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

    /// <summary>Resultado de la consulta: los datos o el motivo por el que no se obtuvieron.</summary>
    public sealed record RucLookupOutcome(RucLookupData? Data, LookupFailure? Failure)
    {
        public static RucLookupOutcome Found(RucLookupData data) => new(data, null);
        public static RucLookupOutcome Failed(LookupFailure failure) => new(null, failure);
    }
}
