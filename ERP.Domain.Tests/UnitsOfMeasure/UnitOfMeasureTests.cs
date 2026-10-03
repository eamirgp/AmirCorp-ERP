using ERP.Domain.Common;
using static ERP.Domain.Tests.TestData;

namespace ERP.Domain.Tests.UnitsOfMeasure
{
    public class UnitOfMeasureTests
    {
        [Fact]
        public void La_unidad_del_stock_no_se_desactiva()
        {
            var each = Each();

            Assert.Equal("No se puede desactivar Unidad: es la unidad en que se cuenta el stock.", each.DeactivateError(0));
            Assert.Throws<DomainException>(() => each.Deactivate(0));
        }

        [Fact]
        public void Una_unidad_que_usa_un_producto_no_se_desactiva()
        {
            Assert.NotNull(Box().DeactivateError(1));
            Assert.Null(Box().DeactivateError(0));
        }

        [Fact]
        public void El_nombre_no_puede_identificar_a_otra_unidad()
        {
            var box = Box();
            var dozen = Dozen();
            var units = new[] { box, dozen };

            // "docena", "DOCENA" o "dzn": la planilla de Excel no sabría cuál es.
            Assert.NotNull(UnitOfMeasureNameError("docena", box, units));
            Assert.NotNull(UnitOfMeasureNameError("DZN", box, units));
            Assert.Null(UnitOfMeasureNameError("Caja grande", box, units));
            // Su propio nombre sí.
            Assert.Null(UnitOfMeasureNameError("caja", box, units));
        }

        [Fact]
        public void El_nombre_se_guarda_sin_espacios_de_sobra()
        {
            var box = Box();

            box.UpdateName("  Caja   grande ", [box]);

            Assert.Equal("Caja grande", box.Name);
        }

        [Fact]
        public void Se_reconoce_por_nombre_codigo_o_nombre_SUNAT_sin_tildes()
        {
            var unit = Unit("BG", "Bolsa", null);

            Assert.True(unit.IsKnownAs(" bolsá "));
            Assert.True(unit.IsKnownAs("bg"));
            Assert.False(unit.IsKnownAs("caja"));
        }

        private static string? UnitOfMeasureNameError(string name, Domain.UnitsOfMeasure.UnitOfMeasure unit, IReadOnlyCollection<Domain.UnitsOfMeasure.UnitOfMeasure> units) =>
            Domain.UnitsOfMeasure.UnitOfMeasure.NameError(name, unit.Id, units);
    }
}
