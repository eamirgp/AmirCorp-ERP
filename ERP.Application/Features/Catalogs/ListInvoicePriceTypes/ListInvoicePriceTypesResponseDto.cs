using ERP.Domain.Catalogs;

namespace ERP.Application.Features.Catalogs.ListInvoicePriceTypes
{
    public sealed record ListInvoicePriceTypesResponseDto(
        InvoicePriceType InvoicePriceType,
        string Description
        );
}
