using ERP.Application.Features.Products.SupplierCodes;
using ERP.Domain.Catalogs;

namespace ERP.Application.Features.Products.GetProduct
{
    public sealed record GetProductResponseDto(
        Guid Id,
        string Code,
        string Name,
        string UnitOfMeasureCode,
        string UnitOfMeasureName,
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
        public string IgvAffectationDescription => IgvAffectation.Description;
    }
}
