using ERP.Application.Common.Responses;
using ERP.Application.Contracts.Infrastructure;
using ERP.Application.Contracts.Persistence.Queries;
using ERP.Application.Features.Products.ProductImport;

namespace ERP.Application.Features.Products.GetProductImportTemplate
{
    internal sealed class GetProductImportTemplateUseCase : IGetProductImportTemplateUseCase
    {
        private readonly IProductSpreadsheet _spreadsheet;
        private readonly IUnitOfMeasureQueries _unitOfMeasureQueries;

        public GetProductImportTemplateUseCase(IProductSpreadsheet spreadsheet, IUnitOfMeasureQueries unitOfMeasureQueries)
        {
            _spreadsheet = spreadsheet;
            _unitOfMeasureQueries = unitOfMeasureQueries;
        }

        // La lista desplegable de unidades trae solo las activas.
        public async Task<FileDto> ExecuteAsync()
        {
            var units = await _unitOfMeasureQueries.ListActiveAsync();
            return new FileDto(_spreadsheet.Write([], units.Select(u => u.Name).ToArray(), ProductSheet.MaxRows), ProductSheet.TemplateFileName);
        }
    }
}
