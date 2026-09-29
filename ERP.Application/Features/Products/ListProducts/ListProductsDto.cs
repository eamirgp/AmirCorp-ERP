namespace ERP.Application.Features.Products.ListProducts
{
    public sealed record ListProductsDto(
        int Page,
        int PageSize,
        string? SearchTerm,
        bool? IsActive,
        ProductSortBy SortBy,
        bool SortDescending
        )
    {
        /// <summary>Orden de la lista cuando la pantalla no pide uno: por nombre, de la A a la Z.</summary>
        public const ProductSortBy DefaultSortBy = ProductSortBy.Name;
        public const bool DefaultSortDescending = false;
    }
}
