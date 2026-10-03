using ERP.Application.Features.Products.ExportProducts;
using ERP.Application.Features.Products.ListProducts;

namespace ERP.Api.Controllers.Products.Requests
{
    /// <summary>Filtros y orden de la lista de productos. Sin filtros se exportan todos.</summary>
    public sealed record ExportProductsRequest(
        string? SearchTerm,
        bool? IsActive,
        ProductSortBy? SortBy,
        bool? SortDescending
        )
    {

        public ExportProductsDto ToDto() =>
            new(
                SearchTerm,
                IsActive,
                SortBy ?? ListProductsDto.DefaultSortBy,
                SortBy is null ? ListProductsDto.DefaultSortDescending : SortDescending ?? false
                );
    }
}
