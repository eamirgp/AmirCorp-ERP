using ERP.Application.Common.Interfaces;

namespace ERP.Application.Features.Products.ExportProducts
{
    /// <summary>Todos los productos en el formato de la plantilla (Excel), para editarlos y volver a subirlos.</summary>
    public interface IExportProductsUseCase : IQueryUseCase<byte[]> { }
}
