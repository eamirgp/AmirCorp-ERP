namespace ERP.Application.Features.Products.ListProducts
{
    public sealed record ListProductsDto(
        int Page,
        int PageSize,
        string? SearchTerm,
        bool? IsActive,
        ProductSortBy SortBy,
        bool SortDescending
        );
}
