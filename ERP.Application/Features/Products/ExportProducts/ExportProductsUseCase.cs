using ERP.Application.Contracts.Infrastructure;
using ERP.Application.Contracts.Persistence.Queries;
using ERP.Domain.Catalogs;

namespace ERP.Application.Features.Products.ExportProducts
{
    internal sealed class ExportProductsUseCase : IExportProductsUseCase
    {
        private readonly IProductQueries _productQueries;
        private readonly IUnitOfMeasureQueries _unitOfMeasureQueries;
        private readonly IProductSpreadsheet _spreadsheet;

        public ExportProductsUseCase(IProductQueries productQueries, IUnitOfMeasureQueries unitOfMeasureQueries, IProductSpreadsheet spreadsheet)
        {
            _productQueries = productQueries;
            _unitOfMeasureQueries = unitOfMeasureQueries;
            _spreadsheet = spreadsheet;
        }

        public async Task<byte[]> ExecuteAsync(ExportProductsDto request)
        {
            var products = await _productQueries.ListForExportAsync(request);
            var units = await _unitOfMeasureQueries.ListActiveAsync();

            // La primera fila de la planilla es la cabecera: los productos empiezan en la fila 2.
            var rows = products
                .Select((p, i) => new ProductSheetRow(i + 2, p.Code, p.Name, p.UnitOfMeasureName, p.IgvAffectation.Description, p.SalePrice, null))
                .ToArray();

            return _spreadsheet.Write(rows, units.Select(u => u.Name).ToArray());
        }
    }
}
