using ERP.Domain.UnitsOfMeasure;

namespace ERP.Application.Features.UnitsOfMeasure
{
    /// <summary>Mensajes cuando se elige una unidad que no existe o que la empresa no usa.</summary>
    internal static class UnitOfMeasureRules
    {
        /// <summary>Error si la unidad no existe o está desactivada; null si se puede usar.</summary>
        public static string? CheckUsable(UnitOfMeasure? unit, string code) =>
            unit is null
                ? $"La unidad de medida '{code.Trim()}' no existe en el catálogo de SUNAT."
                : !unit.IsActive
                    ? $"La unidad de medida '{unit.Name}' está desactivada. Actívala en Administración › Unidades de medida."
                    : null;
    }
}
