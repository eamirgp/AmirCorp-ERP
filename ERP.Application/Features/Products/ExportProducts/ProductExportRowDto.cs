using ERP.Domain.Catalogs;

namespace ERP.Application.Features.Products.ExportProducts
{
    /// <summary>Datos de un producto para exportarlo a la planilla.</summary>
    public sealed record ProductExportRowDto(
        string Code,
        string Name,
        UnitOfMeasure UnitOfMeasure,
        IgvAffectation IgvAffectation,
        decimal SalePrice
        );
}
