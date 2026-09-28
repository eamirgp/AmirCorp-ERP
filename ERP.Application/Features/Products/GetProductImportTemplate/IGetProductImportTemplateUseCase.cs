using ERP.Application.Common.Interfaces;

namespace ERP.Application.Features.Products.GetProductImportTemplate
{
    /// <summary>Plantilla vacía (Excel) para la carga masiva de productos.</summary>
    public interface IGetProductImportTemplateUseCase : IQueryUseCase<byte[]> { }
}
