using ERP.Domain.Partners.Enums;

namespace ERP.Application.Features.Partners.ListBusinessPartners
{
    public sealed record ListBusinessPartnersDto(
        int Page,
        int PageSize,
        string? SearchTerm,
        bool? IsActive,
        PartnerRoleFilter? PartnerRoleFilter,
        IdentityDocumentType? IdentityDocumentType,
        BusinessPartnerSortBy SortBy,
        bool SortDescending
        );
}
