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

        /// <summary>Si un proveedor puede tener este documento: RUC o extranjero sí; DNI no (no emite facturas).</summary>
        public bool CanBeSupplier => IdentityDocumentType.CanIssueTaxDocuments;
    }
}
