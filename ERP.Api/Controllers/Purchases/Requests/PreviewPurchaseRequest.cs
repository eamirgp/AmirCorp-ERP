using ERP.Application.Features.Purchases.PreviewPurchase;
using ERP.Domain.Catalogs;

namespace ERP.Api.Controllers.Purchases.Requests
{
    /// <summary>
    /// Compra a medio llenar. No se valida: las líneas incompletas simplemente no se calculan.
    /// </summary>
    public sealed record PreviewPurchaseRequest(
        InvoicePriceType? InvoicePriceType,
        IReadOnlyCollection<PreviewPurchaseLineRequest>? Lines
        )
    {
        public PreviewPurchaseDto ToDto() =>
            new(
                InvoicePriceType,
                (Lines ?? []).Select(l => new PreviewPurchaseLineDto(
                    l.InvoiceIgvAffectation,
                    l.InvoiceUnitOfMeasure,
                    l.InvoiceQuantity,
                    l.InvoiceAmount,
                    l.ConversionFactor
                    )).ToArray()
                );
    }

    public sealed record PreviewPurchaseLineRequest(
        IgvAffectation? InvoiceIgvAffectation,
        UnitOfMeasure? InvoiceUnitOfMeasure,
        decimal? InvoiceQuantity,
        decimal? InvoiceAmount,
        decimal? ConversionFactor
        );
}
