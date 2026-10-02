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
        // El proveedor: uno ya registrado (SupplierId) o uno nuevo que se registra con la compra (NewSupplier).
        Guid? SupplierId,
        CreatePurchaseNewSupplierDto? NewSupplier,
        IReadOnlyCollection<CreatePurchaseLineDto> Lines
        );

    /// <summary>Proveedor que todavía no existe: se registra junto con la compra, o no se registra ninguno de los dos.</summary>
    public sealed record CreatePurchaseNewSupplierDto(string Ruc, string Name);
}
