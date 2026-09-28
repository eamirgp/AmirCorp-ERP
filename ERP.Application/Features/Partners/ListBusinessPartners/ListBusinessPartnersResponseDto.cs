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
        bool IsActive
        )
    {
        public string IdentityDocumentTypeDescription => IdentityDocumentType.Description;
        public string CountryName => Country.Name;
    }
}
