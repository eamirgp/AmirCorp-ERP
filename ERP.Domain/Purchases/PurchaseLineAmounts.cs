namespace ERP.Domain.Purchases
{
    /// <summary>
    /// Montos calculados de una línea de compra (ver <see cref="PurchaseLine.Calculate"/>).
    /// </summary>
    public sealed record PurchaseLineAmounts(
        decimal InvoiceUnitValue,
        decimal InvoiceUnitPrice,
        // Unidades por cada unidad de la factura que se aplicaron (la fija del catálogo o la de la factura).
        decimal ConversionFactor,
        decimal InventoryQuantity,
        decimal InventoryUnitCost,
        decimal BaseAmount,
        decimal IgvAmount,
        decimal Total
        );
}
