using ERP.Application.Common.Results;
using ERP.Application.Contracts.Infrastructure;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Application.Features.Products.ProductImport;

namespace ERP.Application.Features.Products.ImportProducts
{
    internal sealed class ImportProductsUseCase : IImportProductsUseCase
    {
        private const int MaxErrorsShown = 20;

        private readonly IProductSpreadsheet _spreadsheet;
        private readonly ProductImportPlanner _planner;
        private readonly IUnitOfWork _unitOfWork;

        public ImportProductsUseCase(IProductSpreadsheet spreadsheet, ProductImportPlanner planner, IUnitOfWork unitOfWork)
        {
            _spreadsheet = spreadsheet;
            _planner = planner;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<ProductImportResultDto>> ExecuteAsync(ImportProductsDto request)
        {
            var sheet = _spreadsheet.Read(request.File);
            if (sheet.Error is not null)
                return Result<ProductImportResultDto>.Failure([sheet.Error], ErrorType.BadRequest);

            if (sheet.Rows.Count == 0)
                return Result<ProductImportResultDto>.Failure(["El archivo no tiene productos."], ErrorType.BadRequest);

            var plan = await _planner.BuildAsync(sheet.Rows, request.UpdateExisting);

            var errors = plan
                .Where(e => e.Action == ProductImportAction.Error)
                .SelectMany(e => e.Errors.Select(message => $"Fila {e.RowNumber}: {message}"))
                .ToList();

            if (errors.Count > 0)
            {
                var shown = errors.Take(MaxErrorsShown).ToList();
                if (errors.Count > MaxErrorsShown)
                    shown.Add($"Y {errors.Count - MaxErrorsShown} errores más. Revisa el archivo antes de volver a subirlo.");
                return Result<ProductImportResultDto>.Failure(shown, ErrorType.BadRequest);
            }

            var created = plan.Count(e => e.Action == ProductImportAction.Create);
            var updated = plan.Count(e => e.Action == ProductImportAction.Update);

            if (created + updated == 0)
                return Result<ProductImportResultDto>.Failure(["No hay productos para crear ni actualizar."], ErrorType.BadRequest);

            _planner.Apply(plan);
            await _unitOfWork.SaveChangesAsync();

            return Result<ProductImportResultDto>.Success(new ProductImportResultDto(created, updated));
        }
    }
}
