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
    }
}
