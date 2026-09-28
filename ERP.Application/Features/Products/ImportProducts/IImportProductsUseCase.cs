using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Results;
using ERP.Application.Features.Products.ProductImport;

namespace ERP.Application.Features.Products.ImportProducts
{
    /// <summary>
    /// Confirma la carga masiva. Vuelve a leer y validar el archivo desde cero (no confía en la vista previa)
    /// y guarda todo en una sola transacción: si una fila tiene errores, no se guarda ninguna.
    /// </summary>
    public interface IImportProductsUseCase : IUseCase<ImportProductsDto, Result<ProductImportResultDto>> { }
}
