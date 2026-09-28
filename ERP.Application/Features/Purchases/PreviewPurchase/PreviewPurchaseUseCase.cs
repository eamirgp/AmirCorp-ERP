using ERP.Domain.Catalogs;
using ERP.Domain.Common;
using ERP.Domain.Purchases;

namespace ERP.Application.Features.Purchases.PreviewPurchase
{
    /// <summary>
    /// Calcula los montos de una compra mientras se llena, con la misma fórmula del dominio que usa el registro.
    /// No guarda nada ni consulta la base de datos.
    /// </summary>
    internal sealed class PreviewPurchaseUseCase : IPreviewPurchaseUseCase
    {
        public Task<PreviewPurchaseResponseDto> ExecuteAsync(PreviewPurchaseDto request)
        {
            var results = new List<PreviewPurchaseLineResponseDto>();
            var calculated = new List<PurchaseLineAmounts>();
            var lineNumber = 0;

            foreach (var line in request.Lines)
            {
                lineNumber++;

                // Si la unidad tiene un factor fijo (NIU = 1, DZN = 12) y aún no se envió, se usa ese.
                var conversionFactor = line.ConversionFactor ?? line.InvoiceUnitOfMeasure?.FixedConversionFactor;

                if (request.InvoicePriceType is null
                    || line.InvoiceIgvAffectation is null
                    || line.InvoiceUnitOfMeasure is null
                    || line.InvoiceQuantity is null
                    || line.InvoiceAmount is null
                    || conversionFactor is null)
                {
                    results.Add(new PreviewPurchaseLineResponseDto(lineNumber, null, null, null, null, null, null));
                    continue;
                }

                try
                {
                    var amounts = PurchaseLine.Calculate(
                        request.InvoicePriceType.Value,
                        line.InvoiceIgvAffectation.Value,
                        line.InvoiceUnitOfMeasure.Value,
                        line.InvoiceQuantity.Value,
                        line.InvoiceAmount.Value,
                        conversionFactor.Value
                        );

                    calculated.Add(amounts);
                    results.Add(new PreviewPurchaseLineResponseDto(
                        lineNumber,
                        amounts.BaseAmount,
                        amounts.IgvAmount,
                        amounts.Total,
                        amounts.InventoryQuantity,
                        amounts.InventoryUnitCost,
                        null
                        ));
                }
                catch (DomainException ex)
                {
                    results.Add(new PreviewPurchaseLineResponseDto(lineNumber, null, null, null, null, null, ex.Message));
                }
            }

            var totals = Purchase.CalculateTotals(calculated.Select(a => (a.BaseAmount, a.IgvAmount)));

            return Task.FromResult(new PreviewPurchaseResponseDto(results, totals.TotalBaseAmount, totals.TotalIgvAmount, totals.Total));
        }
    }
}
