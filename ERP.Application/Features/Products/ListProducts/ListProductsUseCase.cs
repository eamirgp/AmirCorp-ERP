using ERP.Application.Common.Pagination;
using ERP.Application.Contracts.Persistence.Queries;

namespace ERP.Application.Features.Products.ListProducts
{
    internal sealed class ListProductsUseCase : IListProductsUseCase
    {
        private readonly IProductQueries _productQueries;

        public ListProductsUseCase(IProductQueries productQueries) => _productQueries = productQueries;

        public async Task<PagedResult<ListProductsResponseDto>> ExecuteAsync(ListProductsDto request) =>
            await _productQueries.ListProductsAsync(request);
    }
}
