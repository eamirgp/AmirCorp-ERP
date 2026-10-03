using ERP.Application.Common.Formatting;
using ERP.Domain.Catalogs;
using ERP.Domain.Purchases;

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
        InvoicePriceType InvoicePriceType,
        decimal InvoiceUnitValue,
        decimal InvoiceUnitPrice,
        decimal ConversionFactor,
        decimal InventoryQuantity,
        decimal InventoryUnitCost,
        decimal BaseAmount,
        decimal IgvAmount,
        decimal Total
        )
    {
        public string InvoiceIgvAffectationDescription => InvoiceIgvAffectation.Description;

        /// <summary>El monto unitario como venía en la factura: el valor (sin IGV) o el precio (con IGV), según la compra.</summary>
        public decimal InvoiceUnitAmount => PurchaseLine.InvoiceAmountFor(InvoicePriceType, InvoiceUnitValue, InvoiceUnitPrice);

        /// <summary>Lo que entró al inventario: "120 und. (24 por caja) · costo 5.00 c/u". Sin conversión, solo las unidades.</summary>
        public string InventoryDescription =>
            PurchaseLineText.Inventory(InventoryQuantity, ConversionFactor, InvoiceUnitOfMeasureName, InventoryUnitCost);
    }
}
