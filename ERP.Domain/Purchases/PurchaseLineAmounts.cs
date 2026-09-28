namespace ERP.Domain.Purchases
{
    /// <summary>
    /// Montos calculados de una línea de compra (ver <see cref="PurchaseLine.Calculate"/>).
    /// </summary>
    public sealed record PurchaseLineAmounts(
        decimal InvoiceUnitValue,
        decimal InvoiceUnitPrice,
        decimal InventoryQuantity,
        decimal InventoryUnitCost,
        decimal BaseAmount,
        decimal IgvAmount,
        decimal Total
        );
}
