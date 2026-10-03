using ERP.Application.Common.Formatting;
using ERP.Domain.Catalogs;

namespace ERP.Application.Features.Purchases.GetPurchase
{
    public sealed record GetPurchaseLineResponseDto(
        int LineNumber,
        Guid ProductId,
        string ProductCode,
        string ProductName,
        // Código del producto tal como venía en la factura del proveedor (null si no traía).
        string? SupplierProductCode,
        IgvAffectation InvoiceIgvAffectation,
        string InvoiceUnitOfMeasureCode,
        string InvoiceUnitOfMeasureName,
        decimal InvoiceQuantity,
        decimal InvoiceUnitAmount,
        decimal ConversionFactor,
        decimal InventoryQuantity,
        decimal InventoryUnitCost,
        decimal BaseAmount,
        decimal IgvAmount,
        decimal Total
        )
    {
        public string InvoiceIgvAffectationDescription => InvoiceIgvAffectation.Description;

        /// <summary>Lo que entró al inventario: "120 und. (24 por caja) · costo 5.00 c/u". Sin conversión, solo las unidades.</summary>
        public string InventoryDescription =>
            NumberText.Decimal(InventoryQuantity) + " und."
            + (ConversionFactor == 1 ? "" : $" ({NumberText.Decimal(ConversionFactor)} por {InvoiceUnitOfMeasureName.ToLowerInvariant()})")
            + " · costo " + NumberText.Cost(InventoryUnitCost) + " c/u";
    }
}
