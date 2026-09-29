using ERP.Application.Features.Products.SupplierCodes;
using ERP.Domain.Catalogs;

namespace ERP.Application.Features.Products.CreateProduct
{
    public sealed record CreateProductDto(
        string Code,
        string Name,
        string UnitOfMeasureCode,
        IgvAffectation IgvAffectation,
        decimal SalePrice,
        IReadOnlyCollection<ProductSupplierCodeDto> SupplierCodes
        );
}
