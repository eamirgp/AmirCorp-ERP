using ERP.Application.Features.Products.SupplierCodes;
using ERP.Domain.Catalogs;

namespace ERP.Application.Features.Products.UpdateProduct
{
    public sealed record UpdateProductDto(
        Guid Id,
        string Code,
        string Name,
        UnitOfMeasure UnitOfMeasure,
        IgvAffectation IgvAffectation,
        decimal SalePrice,
        // Lista completa: los códigos que no vienen se quitan.
        IReadOnlyCollection<ProductSupplierCodeDto> SupplierCodes,
        uint RowVersion
        );
}
