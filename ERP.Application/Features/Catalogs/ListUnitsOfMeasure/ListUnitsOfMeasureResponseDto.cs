using ERP.Domain.Catalogs;

namespace ERP.Application.Features.Catalogs.ListUnitsOfMeasure
{
    public sealed record ListUnitsOfMeasureResponseDto(
        UnitOfMeasure UnitOfMeasure,
        string Description
        )
    {
        /// <summary>
        /// Factor de conversión obligatorio para esta unidad en las compras (NIU = 1, DZN = 12),
        /// o null si lo define cada compra (por ejemplo, cuántas unidades trae una caja).
        /// </summary>
        public decimal? FixedConversionFactor => UnitOfMeasure.FixedConversionFactor;
    }
}
