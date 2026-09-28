using ERP.Domain.Catalogs;

namespace ERP.Application.Features.Products.ListProducts
{
    public sealed record ListProductsResponseDto(
        Guid Id,
        string Code,
        string Name,
        UnitOfMeasure UnitOfMeasure,
        IgvAffectation IgvAffectation,
        decimal SalePrice,
        bool IsActive
        )
    {
        public string UnitOfMeasureDescription => UnitOfMeasure.Description;
        public string IgvAffectationDescription => IgvAffectation.Description;
    }
}
