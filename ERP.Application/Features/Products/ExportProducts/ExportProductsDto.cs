using ERP.Application.Features.Products.ListProducts;

namespace ERP.Application.Features.Products.ExportProducts
{
    /// <summary>Mismos filtros y orden que la lista de productos: se exporta lo que se ve en la pantalla.</summary>
    public sealed record ExportProductsDto(
        string? SearchTerm,
        bool? IsActive,
        ProductSortBy SortBy,
        bool SortDescending
        )
    {
        /// <summary>Si se exporta solo lo filtrado: el archivo se llama "productos-filtrados-…".</summary>
        public bool HasFilters => !string.IsNullOrWhiteSpace(SearchTerm) || IsActive is not null;
    }
}
