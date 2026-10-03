using ERP.Domain.Catalogs;
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

        [Theory]
        [InlineData(TaxDocumentType.Factura, "F001")]
        [InlineData(TaxDocumentType.Factura, "E001")]
        [InlineData(TaxDocumentType.Factura, "0001")]
        [InlineData(TaxDocumentType.Boleta, "B001")]
        [InlineData(TaxDocumentType.Boleta, "EB01")]
        [InlineData(TaxDocumentType.Boleta, " b001 ")]
        public void La_serie_corresponde_al_comprobante(TaxDocumentType type, string serie) =>
            Assert.Null(Purchase.SerieError(type, serie));

        [Theory]
        [InlineData(TaxDocumentType.Factura, "B001")]
        [InlineData(TaxDocumentType.Factura, "EB01")]
        [InlineData(TaxDocumentType.Boleta, "F001")]
        [InlineData(TaxDocumentType.Factura, "F01")]
        [InlineData(TaxDocumentType.Factura, "FÑ01")]
        public void Una_serie_de_otro_comprobante_o_mal_escrita_se_rechaza(TaxDocumentType type, string serie) =>
            Assert.NotNull(Purchase.SerieError(type, serie));

        [Fact]
        public void El_numero_se_completa_con_ceros_y_no_puede_ser_cero()
        {
            Assert.Equal("00000410", Purchase.NormalizeNumber("410"));
            Assert.NotNull(Purchase.NumberError("0000"));
            Assert.NotNull(Purchase.NumberError("123456789"));
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
