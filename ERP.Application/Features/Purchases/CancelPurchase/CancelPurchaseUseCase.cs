using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;

namespace ERP.Application.Features.Purchases.CancelPurchase
{
    internal sealed class CancelPurchaseUseCase : ICancelPurchaseUseCase
    {
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IStockEntryRepository _stockEntryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CancelPurchaseUseCase(
            IPurchaseRepository purchaseRepository,
            IStockEntryRepository stockEntryRepository,
            IUnitOfWork unitOfWork
            )
        {
            _purchaseRepository = purchaseRepository;
            _stockEntryRepository = stockEntryRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> ExecuteAsync(CancelPurchaseDto request)
        {
            var purchase = await _purchaseRepository.GetByIdWithLinesAsync(request.Id);
            if (purchase is null)
                return Result.Failure(["La compra no existe."], ErrorType.NotFound);

            if (purchase.IsCancelled)
                return Result.Failure(["La compra ya se encuentra anulada."], ErrorType.BadRequest);

            var purchaseLineIds = purchase.Lines.Select(l => l.Id).ToArray();
            var stockEntries = await _stockEntryRepository.GetByPurchaseLineIdsAsync(purchaseLineIds);

            if (stockEntries.Any(se => !se.IsIntact))
                return Result.Failure(
                    ["No se puede anular la compra porque su mercadería ya tuvo movimientos de salida."],
                    ErrorType.Conflict
                    );

            purchase.Cancel(request.CancellationReason);

            _stockEntryRepository.RemoveRange(stockEntries);

            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
    }
}