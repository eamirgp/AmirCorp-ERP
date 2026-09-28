using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Pagination;

namespace ERP.Application.Features.Purchases.ListPurchases
{
    public interface IListPurchasesUseCase : IQueryUseCase<ListPurchasesDto, PagedResult<ListPurchasesResponseDto>> { }
}
