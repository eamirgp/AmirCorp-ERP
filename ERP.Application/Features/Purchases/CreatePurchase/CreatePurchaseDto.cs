using ERP.Domain.Catalogs;

namespace ERP.Application.Features.Purchases.CreatePurchase
{
    public sealed record CreatePurchaseDto(
        Guid CompanyId,
        TaxDocumentType TaxDocumentType,
        string Serie,
        string Number,
        DateOnly IssueDate,
        Currency Currency,
        decimal? ExchangeRate,
        InvoicePriceType InvoicePriceType,
        Guid SupplierId,
        IReadOnlyCollection<CreatePurchaseLineDto> Lines
        );
}
