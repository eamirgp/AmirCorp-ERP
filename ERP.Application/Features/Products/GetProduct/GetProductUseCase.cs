using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Queries;

namespace ERP.Application.Features.Products.GetProduct
{
    internal sealed class GetProductUseCase : IGetProductUseCase
    {
        private readonly IProductQueries _productQueries;

        public GetProductUseCase(IProductQueries productQueries) => _productQueries = productQueries;

        public async Task<Result<GetProductResponseDto>> ExecuteAsync(GetProductDto request) =>
            Result<GetProductResponseDto>.FoundOr(await _productQueries.GetProductAsync(request), "El producto no existe.");
    }
}
