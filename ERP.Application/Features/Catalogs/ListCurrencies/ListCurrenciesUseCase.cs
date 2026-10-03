using ERP.Application.Contracts.Infrastructure;
using ERP.Domain.Catalogs;

namespace ERP.Application.Features.Catalogs.ListCurrencies
{
    internal sealed class ListCurrenciesUseCase : IListCurrenciesUseCase
    {
        private readonly IExchangeRateLookup _exchangeRateLookup;

        public ListCurrenciesUseCase(IExchangeRateLookup exchangeRateLookup) => _exchangeRateLookup = exchangeRateLookup;

        public Task<IReadOnlyCollection<ListCurrenciesResponseDto>> ExecuteAsync()
        {
            // SUNAT publica el tipo de cambio del dólar: solo esa moneda se puede consultar, y solo con la consulta configurada.
            IReadOnlyCollection<ListCurrenciesResponseDto> currencies = Enum.GetValues<Currency>()
                .Select(c => new ListCurrenciesResponseDto(c, c.Description, c.HasPublishedExchangeRate && _exchangeRateLookup.IsConfigured))
                .ToArray();

            return Task.FromResult(currencies);
        }
    }
}
