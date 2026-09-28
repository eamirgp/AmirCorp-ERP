using ERP.Domain.Catalogs;
using ERP.Domain.Partners.Enums;

namespace ERP.Application.Features.Purchases.GetPurchase
{
    public sealed record GetPurchaseResponseDto(
        Guid Id,
        Guid CompanyId,
        string? CompanyName,
        TaxDocumentType TaxDocumentType,
        string Serie,
        string Number,
        DateOnly IssueDate,
        Currency Currency,
        decimal? ExchangeRate,
        InvoicePriceType InvoicePriceType,
        Guid SupplierId,
        IdentityDocumentType SupplierIdentityDocumentType,
        string SupplierDocumentNumber,
        string SupplierName,
        decimal TotalBaseAmount,
        decimal TotalIgvAmount,
        decimal Total,
        bool IsCancelled,
        string? CancellationReason,
        DateTime CreatedAt,
        string? CreatedByName,
        DateTime? UpdatedAt,
        string? UpdatedByName,
        IReadOnlyCollection<GetPurchaseLineResponseDto> Lines
        )
    {
        public string TaxDocumentTypeDescription => TaxDocumentType.Description;
        public string CurrencyDescription => Currency.Description;
        public string InvoicePriceTypeDescription => InvoicePriceType.Description;
        public string SupplierIdentityDocumentTypeDescription => SupplierIdentityDocumentType.Description;
        public string FullNumber => Serie + "-" + Number;
    }
}
