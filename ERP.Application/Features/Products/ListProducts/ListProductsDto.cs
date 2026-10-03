namespace ERP.Application.Features.Products.ListProducts
{
    public sealed record ListProductsDto(
        int Page,
        int PageSize,
        string? SearchTerm,
        bool? IsActive,
        ProductSortBy SortBy,
        bool SortDescending,
        // Proveedor con el que se está trabajando (una compra): cada producto trae el código de ese proveedor.
        Guid? SupplierId,
        // Solo los enlazados a ese proveedor: en una compra, lo que ya se le compra (con su código).
        bool OnlySupplierProducts = false,
        // Código de la factura que se quiere enlazar: cada producto dice si se puede elegir para ese código (LinkError).
        string? LinkCode = null
        )
    {
        /// <summary>Orden de la lista cuando la pantalla no pide uno: por nombre, de la A a la Z.</summary>
        public const ProductSortBy DefaultSortBy = ProductSortBy.Name;
        public const bool DefaultSortDescending = false;
    }
}
