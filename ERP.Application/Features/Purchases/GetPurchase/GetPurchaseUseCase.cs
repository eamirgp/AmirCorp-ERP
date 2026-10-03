using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Queries;

namespace ERP.Application.Features.Purchases.GetPurchase
{
    internal sealed class GetPurchaseUseCase : IGetPurchaseUseCase
    {
        private readonly IPurchaseQueries _purchaseQueries;

        public GetPurchaseUseCase(IPurchaseQueries purchaseQueries) => _purchaseQueries = purchaseQueries;

        public async Task<Result<GetPurchaseResponseDto>> ExecuteAsync(GetPurchaseDto request) =>
            Result<GetPurchaseResponseDto>.FoundOr(await _purchaseQueries.GetPurchaseAsync(request), "La compra no existe.");
    }
}
