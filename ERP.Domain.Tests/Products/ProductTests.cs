using ERP.Domain.Products;

namespace ERP.Domain.Tests.Products
{
    /// <summary>El precio de venta (céntimos, sin redondear a escondidas) y el código interno.</summary>
    public class ProductTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(12.9)]
        [InlineData(1500.50)]
        public void Un_precio_con_hasta_dos_decimales_es_valido(decimal price) =>
            Assert.Null(Product.SalePriceError(price));

        [Fact]
        public void Un_precio_con_tres_decimales_se_rechaza_en_vez_de_redondearse()
        {
            Assert.NotNull(Product.SalePriceError(10.555m));
        }

        [Fact]
        public void Un_precio_negativo_se_rechaza()
        {
            Assert.NotNull(Product.SalePriceError(-1m));
        }

        [Fact]
        public void El_codigo_se_guarda_en_mayusculas_y_sin_espacios_alrededor()
        {
            Assert.Equal("EL-1003", Product.NormalizeCode("  el-1003 "));
        }

        [Fact]
        public void Un_codigo_con_salto_de_linea_se_rechaza()
        {
            Assert.Equal("El código interno no puede tener saltos de línea ni tabulaciones.", Product.CodeError("EL-\n1003"));
        }

        [Fact]
        public void Un_codigo_mas_largo_que_el_que_acepta_SUNAT_se_rechaza()
        {
            Assert.NotNull(Product.CodeError(new string('A', Product.CodeMaxLength + 1)));
            Assert.Null(Product.CodeError(new string('A', Product.CodeMaxLength)));
        }
    }
}
