namespace ERP.Domain.Purchases
{
    /// <summary>
    /// Totales de una compra (ver <see cref="Purchase.CalculateTotals"/>).
    /// </summary>
    public sealed record PurchaseTotals(
        decimal TotalBaseAmount,
        decimal TotalIgvAmount,
        decimal Total
        );
}
