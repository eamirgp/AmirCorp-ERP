using ERP.Application.Contracts.Infrastructure;
using ERP.Application.Contracts.Persistence.Queries;

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
        public async Task<byte[]> ExecuteAsync()
        {
            var units = await _unitOfMeasureQueries.ListActiveAsync();
            return _spreadsheet.Write([], units.Select(u => u.Name).ToArray());
        }
    }
}
