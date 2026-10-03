using ERP.Domain.Catalogs;

namespace ERP.Application.Features.Catalogs.ListCurrencies
{
    public sealed record ListCurrenciesResponseDto(
        Currency Currency,
        string Description,
        // Si la pantalla puede traer el tipo de cambio de SUNAT para esta moneda (dólares, con la consulta configurada).
        bool SupportsExchangeRateLookup
        )
    {
        /// <summary>Si una compra en esta moneda lleva tipo de cambio: en soles no aplica y la pantalla bloquea el campo.</summary>
        public bool RequiresExchangeRate => Currency is not Currency.PEN;

        /// <summary>El símbolo de los montos en esta moneda ("S/", "US$"), el mismo que usan los textos de la API.</summary>
        public string Symbol => Currency.Symbol;
    }
}
