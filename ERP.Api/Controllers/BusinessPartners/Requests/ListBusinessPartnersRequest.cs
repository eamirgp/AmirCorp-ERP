using ERP.Application.Common.Pagination;
using ERP.Application.Features.Partners.ListBusinessPartners;
using ERP.Domain.Partners.Enums;

namespace ERP.Api.Controllers.BusinessPartners.Requests
{
    public sealed record ListBusinessPartnersRequest(
        int? Page,
        int? PageSize,
        string? SearchTerm,
        bool? IsActive,
        PartnerRoleFilter? PartnerRoleFilter,
        IdentityDocumentType? IdentityDocumentType,
        BusinessPartnerSortBy? SortBy,
        bool? SortDescending
        )
    {
        public ListBusinessPartnersDto ToDto() =>
            new(
                PaginationDefaults.NormalizedPage(Page),
                PaginationDefaults.NormalizedPageSize(PageSize),
                SearchTerm,
                IsActive,
                PartnerRoleFilter,
                IdentityDocumentType,
                SortBy ?? ListBusinessPartnersDto.DefaultSortBy,
                SortBy is null ? ListBusinessPartnersDto.DefaultSortDescending : SortDescending ?? false
                );
    }
}
