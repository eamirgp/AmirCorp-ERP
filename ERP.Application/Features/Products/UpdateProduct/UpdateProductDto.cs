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
        uint RowVersion
        );
}
