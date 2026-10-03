using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Responses;
using ERP.Application.Common.Results;

namespace ERP.Application.Features.Products.ExportProducts
{
    /// <summary>
    /// Productos en el formato de la plantilla (Excel), para editarlos y volver a subirlos.
    /// Sin filtros exporta todos; con filtros, solo los que coinciden, en el orden de la pantalla. Si son más de los que
    /// admite la carga, responde 400: un archivo que no se podría volver a subir no sirve.
    /// </summary>
    public interface IExportProductsUseCase : IQueryUseCase<ExportProductsDto, Result<FileDto>> { }
}
