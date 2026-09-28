using ERP.Application.Common.Pagination;
using ERP.Application.Features.Purchases.ListPurchases;

namespace ERP.Api.Controllers.Purchases.Requests
{
    public sealed record ListPurchasesRequest(
        int? Page,
        int? PageSize,
        string? SearchTerm,
        Guid? CompanyId,
        PurchaseSortBy? SortBy,
        bool? SortDescending
        )
    {
        public ListPurchasesDto ToDto() =>
            new(
                PaginationDefaults.NormalizedPage(Page),
                PaginationDefaults.NormalizedPageSize(PageSize),
                SearchTerm,
                CompanyId,
                SortBy ?? PurchaseSortBy.CreatedAt,
                SortDescending ?? true
                );
    }
}
