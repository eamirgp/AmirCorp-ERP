namespace ERP.Application.Features.Purchases.PreviewPurchase
{
    /// <summary>
    /// Cálculo de la compra sin guardarla. Los totales suman solo las líneas completas y válidas.
    /// </summary>
    public sealed record PreviewPurchaseResponseDto(
        IReadOnlyCollection<PreviewPurchaseLineResponseDto> Lines,
        decimal TotalBaseAmount,
        decimal TotalIgvAmount,
        decimal Total
        );

    /// <summary>
    /// Resultado de una línea. Si le faltan datos, los montos vienen en null y <see cref="Error"/> también.
    /// Si los datos son inválidos, <see cref="Error"/> trae el mensaje para el usuario.
    /// </summary>
    public sealed record PreviewPurchaseLineResponseDto(
        int LineNumber,
        decimal? BaseAmount,
        decimal? IgvAmount,
        decimal? Total,
        decimal? InventoryQuantity,
        decimal? InventoryUnitCost,
        // "48 und. (24 por caja) · costo 2.00 c/u", el mismo texto del detalle de la compra; null si no se calculó.
        string? InventoryDescription,
        string? Error
        );
}
