using ERP.Application.Common.Interfaces;

namespace ERP.Application.Features.Products.ExportProducts
{
    /// <summary>
    /// Productos en el formato de la plantilla (Excel), para editarlos y volver a subirlos.
    /// Sin filtros exporta todos; con filtros, solo los que coinciden, en el orden de la pantalla.
    /// </summary>
    public interface IExportProductsUseCase : IQueryUseCase<ExportProductsDto, byte[]> { }
}
