using ERP.Application.Common.Pagination;
using ERP.Application.Features.Purchases.GetPurchase;
using ERP.Application.Features.Purchases.ListPurchases;

namespace ERP.Application.Contracts.Persistence.Queries
{
    public interface IPurchaseQueries
    {
        Task<PagedResult<ListPurchasesResponseDto>> ListPurchasesAsync(ListPurchasesDto listPurchasesDto);
        Task<GetPurchaseResponseDto?> GetPurchaseAsync(GetPurchaseDto getPurchaseDto);
    }
}
