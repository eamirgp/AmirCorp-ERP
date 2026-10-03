using ERP.Domain.Purchases;

namespace ERP.Domain.Tests.Purchases
{
    public class PurchaseTests
    {
        private static readonly DateOnly Today = new(2026, 10, 3);

        [Fact]
        public void La_fecha_de_emision_es_requerida_y_no_futura()
        {
            Assert.Equal("La fecha de emisión es requerida.", Purchase.IssueDateError(null, Today));
            Assert.Equal("La fecha de emisión no puede ser mayor a la fecha actual.", Purchase.IssueDateError(Today.AddDays(1), Today));
            Assert.Null(Purchase.IssueDateError(Today, Today));
        }

        [Fact]
        public void Una_fecha_con_el_anio_mal_escrito_se_rechaza()
        {
            Assert.Equal("La fecha de emisión es demasiado antigua. Revisa el año.", Purchase.IssueDateError(new DateOnly(206, 10, 3), Today));
        }

        [Fact]
        public void La_compra_tiene_entre_una_y_500_lineas()
        {
            Assert.Equal("La compra debe tener al menos una línea.", Purchase.LineCountError(0));
            Assert.Null(Purchase.LineCountError(Purchase.MaxLines));
            Assert.Equal("La compra no puede tener más de 500 líneas.", Purchase.LineCountError(Purchase.MaxLines + 1));
        }

        [Fact]
        public void Con_el_maximo_de_lineas_el_total_cabe_en_su_columna()
        {
            // numeric(18,2): 16 dígitos enteros.
            Assert.True(Purchase.MaxLines * PurchaseLine.LineTotalMax <= 9_999_999_999_999_999.99m);
        }

        [Fact]
        public void Un_producto_repetido_avisa_sus_lineas()
        {
            var a = Guid.NewGuid();
            var b = Guid.NewGuid();

            var errors = Purchase.RepeatedProductsErrors([a, b, null, a, null]).ToList();

            Assert.Equal(["Líneas 1, 4: es el mismo producto. Un producto va en una sola línea de la compra."], errors);
        }
    }
}
