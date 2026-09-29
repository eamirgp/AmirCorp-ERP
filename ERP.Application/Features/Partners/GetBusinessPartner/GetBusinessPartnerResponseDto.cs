using ERP.Domain.Catalogs;
using ERP.Domain.Partners.Enums;

namespace ERP.Application.Features.Partners.GetBusinessPartner
{
    public sealed record GetBusinessPartnerResponseDto(
        Guid Id,
        IdentityDocumentType IdentityDocumentType,
        string DocumentNumber,
        string CountryCode,
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
        public string CountryName => Countries.NameOf(CountryCode);
        public string RoleDescription => BusinessPartnerRules.RoleDescription(IsClient, IsSupplier);
    }
}
