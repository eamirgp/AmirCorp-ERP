using ERP.Domain.Catalogs;
using ERP.Domain.Common;
using ERP.Domain.Purchases;
using static ERP.Domain.Tests.TestData;

namespace ERP.Domain.Tests.Purchases
{
    /// <summary>La fórmula de los montos de una línea de compra (la única: la usan el registro y la vista previa).</summary>
    public class PurchaseLineTests
    {
        [Fact]
        public void Con_valor_unitario_el_IGV_se_calcula_sobre_el_subtotal()
        {
            var a = PurchaseLine.Calculate(InvoicePriceType.UnitValue, IgvAffectation.Gravado, Each(), 10m, 8.50m, null);

            Assert.Equal(8.50m, a.InvoiceUnitValue);
            Assert.Equal(10.03m, a.InvoiceUnitPrice);
            Assert.Equal(85.00m, a.BaseAmount);
            Assert.Equal(15.30m, a.IgvAmount);
            Assert.Equal(100.30m, a.Total);
            Assert.Equal(10m, a.InventoryQuantity);
            Assert.Equal(8.50m, a.InventoryUnitCost);
        }

        [Fact]
        public void Con_precio_unitario_el_total_es_el_de_la_factura_y_el_subtotal_sale_de_el()
        {
            var a = PurchaseLine.Calculate(InvoicePriceType.UnitPrice, IgvAffectation.Gravado, Each(), 3m, 10.00m, null);

            Assert.Equal(8.474576m, a.InvoiceUnitValue);
            Assert.Equal(30.00m, a.Total);
            Assert.Equal(25.42m, a.BaseAmount);
            Assert.Equal(4.58m, a.IgvAmount);
            Assert.Equal(a.Total, a.BaseAmount + a.IgvAmount);
            Assert.Equal(8.473333m, a.InventoryUnitCost);
        }

        [Fact]
        public void Una_caja_se_convierte_a_unidades_con_lo_que_dice_la_factura()
        {
            var a = PurchaseLine.Calculate(InvoicePriceType.UnitPrice, IgvAffectation.Gravado, Box(), 5m, 118.00m, 24m);

            Assert.Equal(100m, a.InvoiceUnitValue);
            Assert.Equal(590.00m, a.Total);
            Assert.Equal(500.00m, a.BaseAmount);
            Assert.Equal(90.00m, a.IgvAmount);
            Assert.Equal(24m, a.ConversionFactor);
            Assert.Equal(120m, a.InventoryQuantity);
            Assert.Equal(4.166667m, a.InventoryUnitCost);
        }

        [Fact]
        public void Una_docena_siempre_trae_12()
        {
            var a = PurchaseLine.Calculate(InvoicePriceType.UnitValue, IgvAffectation.Gravado, Dozen(), 2m, 24m, null);

            Assert.Equal(12m, a.ConversionFactor);
            Assert.Equal(24m, a.InventoryQuantity);
            Assert.Equal(2m, a.InventoryUnitCost);
        }

        [Fact]
        public void Inafecto_no_lleva_IGV()
        {
            var a = PurchaseLine.Calculate(InvoicePriceType.UnitValue, IgvAffectation.Inafecto, Each(), 2m, 5m, null);

            Assert.Equal(0m, a.IgvAmount);
            Assert.Equal(10m, a.Total);
            Assert.Equal(a.BaseAmount, a.Total);
        }

        [Fact]
        public void Una_caja_sin_unidades_por_caja_se_rechaza()
        {
            Assert.Equal("Indica cuántas unidades trae cada caja.", PurchaseLine.ConversionFactorError(Box(), null));
            Assert.Throws<DomainException>(() => PurchaseLine.Calculate(InvoicePriceType.UnitValue, IgvAffectation.Gravado, Box(), 1m, 10m, null));
        }

        [Fact]
        public void Una_docena_no_acepta_otra_cantidad()
        {
            Assert.NotNull(PurchaseLine.ConversionFactorError(Dozen(), 10m));
            Assert.Null(PurchaseLine.ConversionFactorError(Dozen(), 12m));
            Assert.Null(PurchaseLine.ConversionFactorError(Dozen(), null));
        }

        [Fact]
        public void Una_unidad_desactivada_no_se_puede_usar()
        {
            var error = PurchaseLine.AmountsError(InvoicePriceType.UnitValue, IgvAffectation.Gravado, Unit("PK", "Paquete", null, isActive: false), 1m, 10m, 6m);

            Assert.Contains("desactivada", error);
        }

        [Theory]
        [InlineData(0, "La cantidad debe ser mayor a cero.")]
        [InlineData(-1, "La cantidad debe ser mayor a cero.")]
        [InlineData(1.1234567, "La cantidad puede tener hasta 6 decimales.")]
        public void Cantidad_invalida(decimal quantity, string expected) =>
            Assert.Equal(expected, PurchaseLine.InvoiceQuantityError(quantity));

        [Fact]
        public void El_monto_dice_si_es_valor_o_precio()
        {
            Assert.Equal("El valor unitario es requerido.", PurchaseLine.InvoiceAmountError(null, InvoicePriceType.UnitValue));
            Assert.Equal("El precio unitario debe ser mayor a cero.", PurchaseLine.InvoiceAmountError(0m, InvoicePriceType.UnitPrice));
        }

        [Fact]
        public void Un_subtotal_de_cero_se_rechaza()
        {
            // 0.001 x 1 = 0.001, que redondeado a centavos es 0.00.
            var error = PurchaseLine.AmountsError(InvoicePriceType.UnitValue, IgvAffectation.Gravado, Each(), 1m, 0.001m, null);

            Assert.Equal("El subtotal de la línea sale 0.00. Revisa la cantidad y el monto.", error);
        }

        [Fact]
        public void El_monto_de_la_factura_es_el_valor_o_el_precio_segun_la_compra()
        {
            Assert.Equal(10m, PurchaseLine.InvoiceAmountFor(InvoicePriceType.UnitValue, 10m, 11.8m));
            Assert.Equal(11.8m, PurchaseLine.InvoiceAmountFor(InvoicePriceType.UnitPrice, 10m, 11.8m));
        }

        [Fact]
        public void Los_totales_de_la_compra_suman_sus_lineas()
        {
            var totals = Purchase.CalculateTotals([(85.00m, 15.30m), (25.42m, 4.58m)]);

            Assert.Equal(110.42m, totals.TotalBaseAmount);
            Assert.Equal(19.88m, totals.TotalIgvAmount);
            Assert.Equal(130.30m, totals.Total);
        }
    }
}
