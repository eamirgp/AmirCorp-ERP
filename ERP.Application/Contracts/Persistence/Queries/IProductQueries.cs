using ERP.Application.Common.Pagination;
using ERP.Application.Features.Products.GetProduct;
using ERP.Application.Features.Products.ListProducts;

namespace ERP.Application.Contracts.Persistence.Queries
{
    public interface IProductQueries
    {
        Task<PagedResult<ListProductsResponseDto>> ListProductsAsync(ListProductsDto listProductsDto);
        Task<GetProductResponseDto?> GetProductAsync(GetProductDto getProductDto);
    }
}
