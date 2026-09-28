using ERP.Domain.Catalogs;

namespace ERP.Application.Features.Purchases.PreviewPurchase
{
    /// <summary>
    /// Datos de una compra que se está llenando. Todo es opcional: las líneas incompletas no se calculan.
    /// </summary>
    public sealed record PreviewPurchaseDto(
        InvoicePriceType? InvoicePriceType,
        IReadOnlyCollection<PreviewPurchaseLineDto> Lines
        );

    public sealed record PreviewPurchaseLineDto(
        IgvAffectation? InvoiceIgvAffectation,
        UnitOfMeasure? InvoiceUnitOfMeasure,
        decimal? InvoiceQuantity,
        decimal? InvoiceAmount,
        decimal? ConversionFactor
        );
}
