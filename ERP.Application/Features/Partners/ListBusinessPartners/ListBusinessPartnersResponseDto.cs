using ERP.Domain.Catalogs;
using ERP.Domain.Partners.Enums;

namespace ERP.Application.Features.Partners.ListBusinessPartners
{
    public sealed record ListBusinessPartnersResponseDto(
        Guid Id,
        IdentityDocumentType IdentityDocumentType,
        string DocumentNumber,
        Country Country,
        string Name,
        bool IsClient,
        bool IsSupplier,
        bool IsActive,
        // Versión del registro: el formulario la devuelve al editar para no pisar cambios de otra persona.
        uint RowVersion
        )
    {
        public string IdentityDocumentTypeDescription => IdentityDocumentType.Description;
        public string CountryName => Country.Name;
        public string RoleDescription => BusinessPartnerRules.RoleDescription(IsClient, IsSupplier);
    }
}
