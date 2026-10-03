using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.Common;
using ERP.Domain.Purchases;
using ERP.Domain.UnitsOfMeasure;

namespace ERP.Application.Features.Purchases.PreviewPurchase
{
    /// <summary>
    /// Calcula los montos de una compra mientras se llena, con la misma fórmula del dominio que usa el registro.
    /// No guarda nada; solo lee las unidades de medida para conocer su factor fijo.
    /// </summary>
    internal sealed class PreviewPurchaseUseCase : IPreviewPurchaseUseCase
    {
        private readonly IUnitOfMeasureRepository _unitOfMeasureRepository;

        public PreviewPurchaseUseCase(IUnitOfMeasureRepository unitOfMeasureRepository) => _unitOfMeasureRepository = unitOfMeasureRepository;

        public async Task<PreviewPurchaseResponseDto> ExecuteAsync(PreviewPurchaseDto request)
        {
            var codes = request.Lines
                .Where(l => !string.IsNullOrWhiteSpace(l.InvoiceUnitOfMeasureCode))
                .Select(l => UnitOfMeasure.NormalizeCode(l.InvoiceUnitOfMeasureCode!))
                .Distinct()
                .ToArray();
            var units = (await _unitOfMeasureRepository.GetByCodesAsync(codes)).ToDictionary(u => u.Code);

            var results = new List<PreviewPurchaseLineResponseDto>();
            var calculated = new List<PurchaseLineAmounts>();
            var lineNumber = 0;

            foreach (var line in request.Lines)
            {
                lineNumber++;

                var unit = string.IsNullOrWhiteSpace(line.InvoiceUnitOfMeasureCode)
                    ? null
                    : units.GetValueOrDefault(UnitOfMeasure.NormalizeCode(line.InvoiceUnitOfMeasureCode));

                // Con una unidad fija (Unidad 1, Docena 12) el dominio pone la cantidad; con una variable (Caja), la
                // línea está incompleta hasta que se indiquen las unidades por caja.
                if (request.InvoicePriceType is null
                    || line.InvoiceIgvAffectation is null
                    || unit is null
                    || line.InvoiceQuantity is null
                    || line.InvoiceAmount is null
                    || (unit.FixedConversionFactor is null && line.ConversionFactor is null))
                {
                    results.Add(new PreviewPurchaseLineResponseDto(lineNumber, null, null, null, null, null, null));
                    continue;
                }

                try
                {
                    var amounts = PurchaseLine.Calculate(
                        request.InvoicePriceType.Value,
                        line.InvoiceIgvAffectation.Value,
                        unit,
                        line.InvoiceQuantity.Value,
                        line.InvoiceAmount.Value,
                        line.ConversionFactor
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

            return new PreviewPurchaseResponseDto(results, totals.TotalBaseAmount, totals.TotalIgvAmount, totals.Total);
        }
    }
}
