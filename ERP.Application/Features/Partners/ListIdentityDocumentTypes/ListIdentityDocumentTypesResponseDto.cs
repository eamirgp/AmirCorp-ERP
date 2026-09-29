using ERP.Domain.Partners.Enums;

namespace ERP.Application.Features.Partners.ListIdentityDocumentTypes
{
    public sealed record ListIdentityDocumentTypesResponseDto(
        IdentityDocumentType IdentityDocumentType,
        string Description
        )
    {
        /// <summary>Si el formulario debe preguntar el país: con DNI o RUC es siempre Perú y no se pregunta.</summary>
        public bool RequiresCountry => !IdentityDocumentType.RequiresPeruvianCountry;
    }
}
