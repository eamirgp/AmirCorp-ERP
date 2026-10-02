using ERP.Domain.Catalogs;

namespace ERP.Application.Features.Catalogs.ListCurrencies
{
    public sealed record ListCurrenciesResponseDto(
        Currency Currency,
        string Description
        )
    {
        /// <summary>Si una compra en esta moneda lleva tipo de cambio: en soles no aplica y la pantalla bloquea el campo.</summary>
        public bool RequiresExchangeRate => Currency is not Currency.PEN;
    }
}
