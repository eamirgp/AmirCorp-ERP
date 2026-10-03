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
        bool? SortDescending,
        // Si se indica, cada producto trae en SupplierCode el código de este proveedor.
        Guid? SupplierId,
        // Solo los productos enlazados a ese proveedor (los que tienen un código suyo). Sin proveedor, ninguno.
        bool? OnlySupplierProducts,
        // En una compra, el código de la factura que se quiere enlazar: cada producto trae LinkError si no se puede.
        string? LinkCode
        )
    {
        public ListProductsDto ToDto() =>
            new(
                PaginationDefaults.NormalizedPage(Page),
                PaginationDefaults.NormalizedPageSize(PageSize),
                SearchTerm,
                IsActive,
                SortBy ?? ListProductsDto.DefaultSortBy,
                SortBy is null ? ListProductsDto.DefaultSortDescending : SortDescending ?? false,
                SupplierId,
                OnlySupplierProducts ?? false,
                string.IsNullOrWhiteSpace(LinkCode) ? null : LinkCode
                );
    }
}
