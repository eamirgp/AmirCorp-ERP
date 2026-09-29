using ERP.Application.Features.Products.SupplierCodes;
using ERP.Domain.Catalogs;

namespace ERP.Application.Features.Products.GetProduct
{
    public sealed record GetProductResponseDto(
        Guid Id,
        string Code,
        string Name,
        UnitOfMeasure UnitOfMeasure,
        IgvAffectation IgvAffectation,
        decimal SalePrice,
        bool IsActive,
        IReadOnlyCollection<ProductSupplierCodeResponseDto> SupplierCodes,
        DateTime CreatedAt,
        string? CreatedByName,
        DateTime? UpdatedAt,
        string? UpdatedByName
        )
    {
        public string UnitOfMeasureDescription => UnitOfMeasure.Description;
        public string IgvAffectationDescription => IgvAffectation.Description;
    }
}
