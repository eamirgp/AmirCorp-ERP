using ERP.Domain.Partners.Enums;

namespace ERP.Application.Features.Partners.ListIdentityDocumentTypes
{
    public sealed record ListIdentityDocumentTypesResponseDto(
        IdentityDocumentType IdentityDocumentType,
        string Description,
        // Si el formulario puede buscar el número en SUNAT y llenar el nombre (RUC, con la consulta configurada).
        bool SupportsLookup
        )
    {
        /// <summary>Si el formulario debe preguntar el país: con DNI o RUC es siempre Perú y no se pregunta.</summary>
        public bool RequiresCountry => !IdentityDocumentType.RequiresPeruvianCountry;

        /// <summary>
        /// Cuántos dígitos tiene el número completo (DNI 8, RUC 11), o null si varía. El formulario lo usa para saber
        /// cuándo está completo y buscarlo en SUNAT sin que el usuario presione nada.
        /// </summary>
        public int? ExactLength => IdentityDocumentType switch
        {
            IdentityDocumentType.Dni => IdentityDocumentTypeExtensions.DniLength,
            IdentityDocumentType.Ruc => IdentityDocumentTypeExtensions.RucLength,
            _ => null
        };

        /// <summary>Si un proveedor puede tener este documento: RUC o extranjero sí; DNI no (no emite facturas).</summary>
        public bool CanBeSupplier => IdentityDocumentType.CanIssueTaxDocuments;

        /// <summary>Si un cliente puede tener este documento: por ahora solo RUC o DNI (ventas nacionales).</summary>
        public bool CanBeClient => IdentityDocumentType.CanBeClient;
    }
}
