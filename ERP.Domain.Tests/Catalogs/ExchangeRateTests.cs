using ERP.Domain.Catalogs;
using ERP.Domain.Common;

namespace ERP.Domain.Tests.Catalogs
{
    public class ExchangeRateTests
    {
        private static readonly DateOnly Monday = new(2026, 9, 28);
        private static readonly DateOnly Friday = new(2026, 9, 25);
        private static readonly DateOnly Saturday = new(2026, 9, 26);

        [Fact]
        public void Aplica_la_publicacion_del_mismo_dia()
        {
            Assert.Equal(Monday, ExchangeRate.ApplicablePublication(Monday, [Friday, Monday]));
        }

        [Fact]
        public void Un_dia_sin_publicacion_usa_la_ultima_anterior()
        {
            Assert.Equal(Friday, ExchangeRate.ApplicablePublication(Saturday, [Friday, Monday]));
        }

        [Fact]
        public void No_aplica_una_publicacion_posterior_ni_una_demasiado_antigua()
        {
            Assert.Null(ExchangeRate.ApplicablePublication(Friday, [Monday]));
            Assert.Null(ExchangeRate.ApplicablePublication(Monday, [Monday.AddDays(-ExchangeRate.MaxDaysBack - 1)]));
            Assert.Equal(Monday.AddDays(-ExchangeRate.MaxDaysBack), ExchangeRate.ApplicablePublication(Monday, [Monday.AddDays(-ExchangeRate.MaxDaysBack)]));
        }

        [Fact]
        public void Un_dia_pasado_sin_publicacion_se_guarda_pero_hoy_no()
        {
            Assert.True(ExchangeRate.StoresDayWithoutPublication(Saturday, Friday, today: Monday));
            Assert.False(ExchangeRate.StoresDayWithoutPublication(Saturday, Friday, today: Saturday));
            Assert.False(ExchangeRate.StoresDayWithoutPublication(Friday, Friday, today: Monday));
        }

        [Theory]
        [InlineData(0, "El tipo de cambio debe ser mayor a cero.")]
        [InlineData(1000.01, "El tipo de cambio es demasiado grande. Revisa que esté bien escrito.")]
        [InlineData(3.7512345, "El tipo de cambio puede tener hasta 6 decimales.")]
        public void Tipo_de_cambio_invalido(decimal rate, string expected) =>
            Assert.Equal(expected, ExchangeRate.RateError(rate));

        [Fact]
        public void Lo_publicado_se_revisa_antes_de_guardarlo()
        {
            Assert.Null(ExchangeRate.PublishedError(Saturday, Friday, 3.75m, 3.76m));
            Assert.NotNull(ExchangeRate.PublishedError(Friday, Saturday, 3.75m, 3.76m));
            Assert.Equal("El tipo de cambio de compra no puede ser mayor al de venta.", ExchangeRate.PublishedError(Friday, Friday, 3.77m, 3.76m));
            Assert.Throws<DomainException>(() => ExchangeRate.Create(Currency.USD, Friday, Friday, 3.77m, 3.76m, "SUNAT", DateTime.UtcNow));
        }

        [Fact]
        public void El_sol_no_tiene_tipo_de_cambio()
        {
            Assert.Throws<DomainException>(() => ExchangeRate.Create(Currency.PEN, Friday, Friday, 1m, 1m, "SUNAT", DateTime.UtcNow));
            Assert.False(Currency.PEN.HasPublishedExchangeRate);
            Assert.True(Currency.USD.HasPublishedExchangeRate);
        }

        [Fact]
        public void No_se_busca_una_fecha_futura()
        {
            Assert.NotNull(ExchangeRate.LookupDateError(Monday, today: Friday));
            Assert.Null(ExchangeRate.LookupDateError(Friday, today: Monday));
        }
    }
}
