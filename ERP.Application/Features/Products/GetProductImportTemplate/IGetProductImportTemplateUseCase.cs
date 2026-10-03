using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Responses;

namespace ERP.Application.Features.Products.GetProductImportTemplate
{
    /// <summary>Plantilla vacía (Excel) para la carga masiva de productos, con el nombre del archivo.</summary>
    public interface IGetProductImportTemplateUseCase : IQueryUseCase<FileDto> { }
}
