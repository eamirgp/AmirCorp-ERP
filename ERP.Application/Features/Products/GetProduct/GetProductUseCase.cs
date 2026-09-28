using ERP.Application.Contracts.Persistence.Queries;

namespace ERP.Application.Features.Products.GetProduct
{
    internal sealed class GetProductUseCase : IGetProductUseCase
    {
        private readonly IProductQueries _productQueries;

        public GetProductUseCase(IProductQueries productQueries) => _productQueries = productQueries;

        public async Task<GetProductResponseDto?> ExecuteAsync(GetProductDto request) =>
            await _productQueries.GetProductAsync(request);
    }
}
