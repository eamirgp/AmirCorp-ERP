using ERP.Application.Common.Pagination;
using ERP.Application.Features.Products.ExportProducts;
using ERP.Application.Features.Products.GetProduct;
using ERP.Application.Features.Products.ListProducts;

namespace ERP.Application.Contracts.Persistence.Queries
{
    public interface IProductQueries
    {
        Task<SortedPagedResult<ListProductsResponseDto, ProductSortBy>> ListProductsAsync(ListProductsDto listProductsDto);
        Task<GetProductResponseDto?> GetProductAsync(GetProductDto getProductDto);
        Task<IReadOnlyCollection<ProductExportRowDto>> ListForExportAsync(ExportProductsDto exportProductsDto);
    }
}
