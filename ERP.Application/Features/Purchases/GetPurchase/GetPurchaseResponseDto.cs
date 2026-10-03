using ERP.Domain.Catalogs;
using ERP.Domain.Partners.Enums;

namespace ERP.Application.Features.Purchases.GetPurchase
{
    public sealed record GetPurchaseResponseDto(
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
        /// <summary>El símbolo de los montos ("S/", "US$"), el mismo del costo de cada línea.</summary>
        public string CurrencySymbol => Currency.Symbol;
        public string InvoicePriceTypeDescription => InvoicePriceType.Description;
        public string SupplierIdentityDocumentTypeDescription => SupplierIdentityDocumentType.Description;
        public string FullNumber => Serie + "-" + Number;
        /// <summary>"Registrada" o "Anulada", junto al título del comprobante.</summary>
        public string StatusDescription => IsCancelled ? "Anulada" : "Registrada";
    }
}
