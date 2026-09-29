namespace ERP.Application.Features.Catalogs.ListUnitsOfMeasure
{
    /// <summary>Unidad activa para elegir en un formulario.</summary>
    public sealed record ListUnitsOfMeasureResponseDto(
        string Code,
        string Name,
        // Factor de conversión obligatorio para esta unidad en las compras (NIU = 1, DZN = 12),
        // o null si lo define cada compra (por ejemplo, cuántas unidades trae una caja).
        decimal? FixedConversionFactor
        );
}
