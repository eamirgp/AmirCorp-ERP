using ERP.Domain.Catalogs;

namespace ERP.Application.Features.Purchases.PreviewPurchase
{
    /// <summary>
    /// Datos de una compra que se está llenando. Todo es opcional: las líneas incompletas no se calculan.
    /// </summary>
    public sealed record PreviewPurchaseDto(
        InvoicePriceType? InvoicePriceType,
        // Solo para mostrar el costo con su símbolo; sin moneda, el costo va sin símbolo.
        Currency? Currency,
        IReadOnlyCollection<PreviewPurchaseLineDto> Lines
        );

    public sealed record PreviewPurchaseLineDto(
        IgvAffectation? InvoiceIgvAffectation,
        string? InvoiceUnitOfMeasureCode,
        decimal? InvoiceQuantity,
        decimal? InvoiceAmount,
        decimal? ConversionFactor
        );
}
