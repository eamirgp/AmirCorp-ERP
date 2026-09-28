using ERP.Application.Common.Interfaces;

namespace ERP.Application.Features.Catalogs.ListInvoicePriceTypes
{
    public interface IListInvoicePriceTypesUseCase : IQueryUseCase<IReadOnlyCollection<ListInvoicePriceTypesResponseDto>> { }
}
