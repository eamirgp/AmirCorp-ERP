using ERP.Application.Common.Pagination;
using ERP.Application.Contracts.Persistence.Queries;

namespace ERP.Application.Features.Purchases.ListPurchases
{
    internal sealed class ListPurchasesUseCase : IListPurchasesUseCase
    {
        private readonly IPurchaseQueries _purchaseQueries;

        public ListPurchasesUseCase(IPurchaseQueries purchaseQueries) => _purchaseQueries = purchaseQueries;

        public async Task<SortedPagedResult<ListPurchasesResponseDto, PurchaseSortBy>> ExecuteAsync(ListPurchasesDto request) =>
            await _purchaseQueries.ListPurchasesAsync(request);
    }
}
