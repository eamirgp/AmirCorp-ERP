using ERP.Application.Common.Pagination;
using ERP.Application.Features.Products.ListProducts;

namespace ERP.Api.Controllers.Products.Requests
{
    public sealed record ListProductsRequest(
        int? Page,
        int? PageSize,
        string? SearchTerm,
        bool? IsActive,
        ProductSortBy? SortBy,
        bool? SortDescending
        )
    {
        public ListProductsDto ToDto() =>
            new(
                PaginationDefaults.NormalizedPage(Page),
                PaginationDefaults.NormalizedPageSize(PageSize),
                SearchTerm,
                IsActive,
                SortBy ?? ListProductsDto.DefaultSortBy,
                SortBy is null ? ListProductsDto.DefaultSortDescending : SortDescending ?? false
                );
    }
}
