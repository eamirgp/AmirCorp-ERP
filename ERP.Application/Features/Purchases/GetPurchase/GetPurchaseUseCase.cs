using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Application.Contracts.Persistence.Queries;

namespace ERP.Application.Features.Purchases.GetPurchase
{
    internal sealed class GetPurchaseUseCase : IGetPurchaseUseCase
    {
        private readonly IPurchaseQueries _purchaseQueries;
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IStockEntryRepository _stockEntryRepository;

        public GetPurchaseUseCase(IPurchaseQueries purchaseQueries, IPurchaseRepository purchaseRepository, IStockEntryRepository stockEntryRepository)
        {
            _purchaseQueries = purchaseQueries;
            _purchaseRepository = purchaseRepository;
            _stockEntryRepository = stockEntryRepository;
        }

        public async Task<Result<GetPurchaseResponseDto>> ExecuteAsync(GetPurchaseDto request)
        {
            var found = await _purchaseQueries.GetPurchaseAsync(request);
            var purchase = found is null ? null : await _purchaseRepository.GetByIdWithLinesAsync(request.Id);
            if (found is null || purchase is null)
                return Result<GetPurchaseResponseDto>.FoundOr(null, "La compra no existe.");

            // Si se puede anular lo decide el dominio (Purchase.CancelError: ya anulada, o su mercadería tuvo salidas): la
            // pantalla solo ofrece "Anular" si no hay motivo en contra. Una anulada ya no tiene ingresos de stock.
            var stockEntries = purchase.IsCancelled ? [] : await _stockEntryRepository.GetByPurchaseLineIdsAsync(purchase.Lines.Select(l => l.Id).ToArray());

            return Result<GetPurchaseResponseDto>.Success(found with { CancelError = purchase.CancelError(stockEntries) });
        }
    }
}
