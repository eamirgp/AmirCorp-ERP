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
        bool IsActive,
        // Versión del producto: el formulario la devuelve al editar para no pisar cambios de otra persona.
        uint RowVersion
        )
    {
        public string UnitOfMeasureDescription => UnitOfMeasure.Description;
        public string IgvAffectationDescription => IgvAffectation.Description;
        public string IgvAffectationShortDescription => IgvAffectation.ShortDescription;
    }
}
