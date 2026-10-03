using System.Reflection;
using ERP.Domain.UnitsOfMeasure;

namespace ERP.Domain.Tests
{
    /// <summary>
    /// Datos para las pruebas. Las unidades de medida no se crean desde el sistema (las cargan las migraciones), así que
    /// aquí se arman con su constructor privado, como lo hace EF Core al leerlas.
    /// </summary>
    internal static class TestData
    {
        public static UnitOfMeasure Unit(string code, string name, decimal? fixedConversionFactor, bool isActive = true) =>
            (UnitOfMeasure)Activator.CreateInstance(
                typeof(UnitOfMeasure),
                BindingFlags.Instance | BindingFlags.NonPublic,
                binder: null,
                args: [Guid.CreateVersion7(), code, name.ToUpperInvariant(), name, fixedConversionFactor, isActive],
                culture: null)!;

        public static UnitOfMeasure Each() => Unit(UnitOfMeasure.BaseUnitCode, "Unidad", 1m);
        public static UnitOfMeasure Dozen() => Unit("DZN", "Docena", 12m);
        public static UnitOfMeasure Box() => Unit("BX", "Caja", null);
    }
}
