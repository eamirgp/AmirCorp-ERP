using ERP.Application.Common.Formatting;

namespace ERP.Application.Features.UnitsOfMeasure.ListAllUnitsOfMeasure
{
    /// <summary>Unidad del catálogo SUNAT para la pantalla de administración, con los textos listos para mostrar.</summary>
    public sealed record UnitOfMeasureListItemDto(
        Guid Id,
        string Code,
        string Name,
        string SunatName,
        decimal? FixedConversionFactor,
        bool IsActive,
        int ProductCount,
        // Versión del registro: el formulario del nombre corto la devuelve al guardar.
        uint RowVersion
        )
    {
        /// <summary>"12 unidades" si siempre trae lo mismo; "Según la compra" si cada factura lo indica.</summary>
        public string ConversionDescription => FixedConversionFactor is { } factor
            ? $"{NumberText.Decimal(factor)} {(factor == 1 ? "unidad" : "unidades")}"
            : "Según la compra";

        /// <summary>"35 productos", "1 producto" o "Ninguno".</summary>
        public string ProductCountDescription => ProductCount switch
        {
            0 => "Ninguno",
            1 => "1 producto",
            _ => $"{NumberText.Integer(ProductCount)} productos"
        };
    }
}
