using ERP.Application.Features.Purchases.PreviewPurchase;
using ERP.Domain.Catalogs;
using ERP.Domain.Purchases;

namespace ERP.Api.Controllers.Purchases.Requests
{
    /// <summary>
    /// Compra a medio llenar. No se valida: las líneas incompletas simplemente no se calculan.
    /// </summary>
    public sealed record PreviewPurchaseRequest(
        InvoicePriceType? InvoicePriceType,
        IReadOnlyCollection<PreviewPurchaseLineRequest>? Lines,
        // La moneda elegida, para mostrar el costo con su símbolo. Opcional.
        Currency? Currency = null
        )
    {
        /// <summary>Solo el máximo de líneas: una compra a medio llenar puede no tener ninguna todavía.</summary>
        public IReadOnlyCollection<string> Validate() =>
            Lines is { Count: > 0 } && Purchase.LineCountError(Lines.Count) is { } error ? [error] : [];

        public PreviewPurchaseDto ToDto() =>
            new(
                InvoicePriceType,
                Currency,
                // Una línea null es una línea vacía: no se calcula, como las incompletas.
                (Lines ?? []).Select(l => new PreviewPurchaseLineDto(
                    l?.InvoiceIgvAffectation,
                    l?.InvoiceUnitOfMeasureCode,
                    l?.InvoiceQuantity,
                    l?.InvoiceAmount,
                    l?.ConversionFactor
                    )).ToArray()
                );
    }

    public sealed record PreviewPurchaseLineRequest(
        IgvAffectation? InvoiceIgvAffectation,
        string? InvoiceUnitOfMeasureCode,
        decimal? InvoiceQuantity,
        decimal? InvoiceAmount,
        decimal? ConversionFactor
        );
}
