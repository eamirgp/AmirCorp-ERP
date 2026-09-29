using ERP.Domain.Catalogs;

namespace ERP.Application.Features.Products.ExportProducts
{
    /// <summary>Datos de un producto para exportarlo a la planilla. La unidad va con su nombre, como en la lista.</summary>
    public sealed record ProductExportRowDto(
        string Code,
        string Name,
        string UnitOfMeasureName,
        IgvAffectation IgvAffectation,
        decimal SalePrice
        );
}
