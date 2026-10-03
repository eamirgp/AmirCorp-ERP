using ERP.Application.Common.Results;
using ERP.Application.Contracts.Infrastructure;
using ERP.Application.Features.Products.ProductImport;

namespace ERP.Application.Features.Products.PreviewProductImport
{
    internal sealed class PreviewProductImportUseCase : IPreviewProductImportUseCase
    {
        private readonly IProductSpreadsheet _spreadsheet;
        private readonly ProductImportPlanner _planner;

        public PreviewProductImportUseCase(IProductSpreadsheet spreadsheet, ProductImportPlanner planner)
        {
            _spreadsheet = spreadsheet;
            _planner = planner;
        }

        public async Task<Result<ProductImportPreviewDto>> ExecuteAsync(ImportProductsDto request)
        {
            var sheet = _spreadsheet.Read(request.File, ProductSheet.MaxRows);
            if (sheet.Error is not null)
                return Result<ProductImportPreviewDto>.Failure([ProductSheet.ReadErrorMessage(sheet.Error.Value)], ErrorType.BadRequest);

            if (sheet.Rows.Count == 0)
                return Result<ProductImportPreviewDto>.Failure(["El archivo no tiene productos."], ErrorType.BadRequest);

            var plan = await _planner.BuildAsync(sheet.Rows, request.UpdateExisting);

            return Result<ProductImportPreviewDto>.Success(new ProductImportPreviewDto(
                plan.Select(e => new ProductImportRowDto(e.RowNumber, e.Code, e.Name, e.Action, e.Errors, e.Changes)).ToArray(),
                plan.Count(e => e.Action == ProductImportAction.Create),
                plan.Count(e => e.Action == ProductImportAction.Update),
                plan.Count(e => e.Action == ProductImportAction.Skip),
                plan.Count(e => e.Action == ProductImportAction.Unchanged),
                plan.Count(e => e.Action == ProductImportAction.Error),
                _planner.VersionOf(plan)
                ));
        }
    }
}
