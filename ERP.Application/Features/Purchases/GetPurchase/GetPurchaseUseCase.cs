using ERP.Application.Contracts.Persistence.Queries;

namespace ERP.Application.Features.Purchases.GetPurchase
{
    internal sealed class GetPurchaseUseCase : IGetPurchaseUseCase
    {
        private readonly IPurchaseQueries _purchaseQueries;

        public GetPurchaseUseCase(IPurchaseQueries purchaseQueries) => _purchaseQueries = purchaseQueries;

        public async Task<GetPurchaseResponseDto?> ExecuteAsync(GetPurchaseDto request) =>
            await _purchaseQueries.GetPurchaseAsync(request);
    }
}
