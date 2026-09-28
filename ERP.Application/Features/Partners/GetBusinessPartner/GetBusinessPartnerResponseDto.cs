using ERP.Domain.Catalogs;
using ERP.Domain.Partners.Enums;

namespace ERP.Application.Features.Partners.GetBusinessPartner
{
    public sealed record GetBusinessPartnerResponseDto(
        Guid Id,
        IdentityDocumentType IdentityDocumentType,
        string DocumentNumber,
        Country Country,
        string Name,
        bool IsClient,
        bool IsSupplier,
        bool IsActive,
        DateTime CreatedAt,
        string? CreatedByName,
        DateTime? UpdatedAt,
        string? UpdatedByName
        )
    {
        public string IdentityDocumentTypeDescription => IdentityDocumentType.Description;
        public string CountryName => Country.Name;
    }
}
