using ERP.Domain.Catalogs;

namespace ERP.Application.Features.Catalogs.ListInvoicePriceTypes
{
    internal sealed class ListInvoicePriceTypesUseCase : IListInvoicePriceTypesUseCase
    {
        private static readonly IReadOnlyCollection<ListInvoicePriceTypesResponseDto> _invoicePriceTypes =
            Enum.GetValues<InvoicePriceType>()
                .Select(ipt => new ListInvoicePriceTypesResponseDto(ipt, ipt.Description))
                .ToArray();

        public Task<IReadOnlyCollection<ListInvoicePriceTypesResponseDto>> ExecuteAsync() =>
            Task.FromResult(_invoicePriceTypes);
    }
}
