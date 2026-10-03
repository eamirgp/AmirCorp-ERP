using ERP.Application.Common.Responses;
using ERP.Application.Common.Results;
using ERP.Application.Contracts.Infrastructure;
using ERP.Application.Contracts.Persistence.Queries;
using ERP.Application.Features.Products.ProductImport;
using ERP.Domain.Catalogs;
using ERP.Domain.Common;

namespace ERP.Application.Features.Products.ExportProducts
{
    internal sealed class ExportProductsUseCase : IExportProductsUseCase
    {
        private readonly IProductQueries _productQueries;
        private readonly IUnitOfMeasureQueries _unitOfMeasureQueries;
        private readonly IProductSpreadsheet _spreadsheet;
        private readonly TimeProvider _timeProvider;

        public ExportProductsUseCase(IProductQueries productQueries, IUnitOfMeasureQueries unitOfMeasureQueries, IProductSpreadsheet spreadsheet, TimeProvider timeProvider)
        {
            _productQueries = productQueries;
            _unitOfMeasureQueries = unitOfMeasureQueries;
            _spreadsheet = spreadsheet;
            _timeProvider = timeProvider;
        }

        public async Task<Result<FileDto>> ExecuteAsync(ExportProductsDto request)
        {
            var products = await _productQueries.ListForExportAsync(request);
            if (products.Count > ProductSheet.MaxRows)
                return Result<FileDto>.Failure([ProductSheet.TooManyToExport(products.Count)], ErrorType.BadRequest);

            var units = await _unitOfMeasureQueries.ListActiveAsync();

            // La primera fila de la planilla es la cabecera: los productos empiezan en la fila 2.
            var rows = products
                .Select((p, i) => new ProductSheetRow(i + 2, p.Code, p.Name, p.UnitOfMeasureName, p.IgvAffectation.Description, p.SalePrice, null))
                .ToArray();

            var content = _spreadsheet.Write(rows, units.Select(u => u.Name).ToArray(), ProductSheet.MaxRows);
            var today = PeruCalendar.Today(_timeProvider.GetUtcNow().UtcDateTime);
            return Result<FileDto>.Success(new FileDto(content, ProductSheet.ExportFileName(request.HasFilters, today)));
        }
    }
}
