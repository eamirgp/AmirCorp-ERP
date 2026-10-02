using ERP.Domain.Catalogs;

namespace ERP.Application.Contracts.Persistence.Commands
{
    public interface IExchangeRateRepository
    {
        Task<ExchangeRate?> GetAsync(Currency currency, DateOnly date);

        /// <summary>El guardado más reciente de esa fecha o anterior, sin ir más atrás de <paramref name="notBefore"/>.</summary>
        Task<ExchangeRate?> LatestOnOrBeforeAsync(Currency currency, DateOnly date, DateOnly notBefore);

        /// <summary>Fechas que ya están guardadas dentro del rango.</summary>
        Task<IReadOnlyCollection<DateOnly>> ExistingDatesAsync(Currency currency, DateOnly from, DateOnly to);

        void Add(ExchangeRate exchangeRate);
    }
}
