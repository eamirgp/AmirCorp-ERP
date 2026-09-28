using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Results;
using ERP.Application.Features.Products.ProductImport;

namespace ERP.Application.Features.Products.PreviewProductImport
{
    /// <summary>Lee la planilla y dice qué pasará con cada fila. No guarda nada.</summary>
    public interface IPreviewProductImportUseCase : IUseCase<ImportProductsDto, Result<ProductImportPreviewDto>> { }
}
