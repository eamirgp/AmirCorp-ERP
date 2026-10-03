using ERP.Domain.Partners;

namespace ERP.Application.Common.Lookup
{
    /// <summary>Datos de SUNAT (RUC) o RENIEC (DNI) para llenar un formulario, con avisos listos para mostrar.</summary>
    public sealed record LookupDocumentResponseDto(
        string DocumentNumber,
        string Name,
        // De dónde vienen los datos: "SUNAT" o "RENIEC".
        string Source,
        // Solo SUNAT los informa; con DNI vienen vacíos.
        string? Status,
        string? Condition,
        string? Address,
        // Avisos para revisar antes de guardar: contribuyente de baja o no habido, o ya registrado en el sistema.
        IReadOnlyCollection<string> Warnings
        )
    {
        /// <summary>
        /// Lo consultado en una línea para la pantalla: "Según SUNAT: ACTIVO · HABIDO" con RUC (sin huecos si falta uno), o
        /// "Según RENIEC: el nombre" con DNI.
        /// </summary>
        public string Summary =>
            $"Según {Source}: "
            + (Status is null && Condition is null
                ? Name
                : TaxpayerStatus.Summary(Status, Condition) ?? "no informa si está activo y habido");

        internal static LookupDocumentResponseDto From(DocumentLookupData data, IReadOnlyCollection<string> warnings) =>
            new(data.DocumentNumber, data.Name, data.Source, data.Ruc?.Status, data.Ruc?.Condition, data.Ruc?.Address, warnings);
    }
}
