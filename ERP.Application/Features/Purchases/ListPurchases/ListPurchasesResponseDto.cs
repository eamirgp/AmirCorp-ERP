using ERP.Domain.Catalogs;

namespace ERP.Application.Features.Purchases.ListPurchases
{
    public sealed record ListPurchasesResponseDto(
        Guid Id,
        Guid CompanyId,
        // Copia guardada en la compra: la razón social que la empresa tenía al registrarla.
        string CompanyName,
        TaxDocumentType TaxDocumentType,
        string Serie,
        string Number,
        DateOnly IssueDate,
        Currency Currency,
        decimal? ExchangeRate,
        string SupplierDocumentNumber,
        string SupplierName,
        decimal Total,
        bool IsCancelled,
        string? CancellationReason
        )
    {
        public string TaxDocumentTypeDescription => TaxDocumentType.Description;
        public string CurrencyDescription => Currency.Description;
        public string FullNumber => Serie + "-" + Number;
    }
}
