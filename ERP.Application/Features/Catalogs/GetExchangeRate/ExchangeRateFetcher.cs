using ERP.Application.Contracts.Infrastructure;
using ERP.Domain.Catalogs;

namespace ERP.Application.Features.Catalogs.GetExchangeRate
{
    /// <summary>
    /// Lo publicado por SUNAT alrededor de una fecha, pedido al servicio externo con la menor cantidad de consultas: el mes
    /// completo (y el anterior si la fecha es de los primeros días y el mes todavía no trae nada hasta ella: 1 de enero,
    /// feriados) y, si el servicio no ofrece el mes, la fecha sola. Lo que no tiene sentido según el dominio
    /// (<see cref="ExchangeRate.PublishedError"/>) se descarta: un dato raro del servicio no termina en un error.
    /// </summary>
    internal sealed class ExchangeRateFetcher
    {
        private readonly IExchangeRateLookup _lookup;

        public ExchangeRateFetcher(IExchangeRateLookup lookup) => _lookup = lookup;

        /// <summary>Los días que puede traer una consulta para esa fecha: desde el mes del día más antiguo que aplica hasta el fin del mes.</summary>
        public static (DateOnly From, DateOnly To) CoveredRange(DateOnly date)
        {
            var oldest = ExchangeRate.OldestApplicable(date);
            return (new DateOnly(oldest.Year, oldest.Month, 1), new DateOnly(date.Year, date.Month, 1).AddMonths(1).AddDays(-1));
        }

        /// <returns>Lo publicado hasta hoy (sin repetir días) y, si no se pudo saber nada de esa fecha, el motivo.</returns>
        public async Task<(IReadOnlyList<ExchangeRateData> Published, LookupFailure? Failure)> FetchAsync(DateOnly date, DateOnly today, CancellationToken ct)
        {
            var monthStart = new DateOnly(date.Year, date.Month, 1);
            var (from, _) = CoveredRange(date);

            var published = Usable(await _lookup.FindMonthAsync(date.Year, date.Month, ct), today);
            if (from < monthStart && published.All(r => r.Date > date))
                published.AddRange(Usable(await _lookup.FindMonthAsync(from.Year, from.Month, ct), today));

            LookupFailure? failure = null;
            if (published.All(r => r.Date > date))
            {
                var single = await _lookup.FindAsync(date, ct);
                if (single.Data is { } data && data.Date <= date)
                {
                    if (IsUsable(data, today))
                        published.Add(data);
                    else
                        // Respondió algo que no se puede usar: es como si no hubiera respondido.
                        failure = LookupFailure.Unavailable;
                }
                else
                    failure = single.Failure ?? LookupFailure.NotFound;
            }

            return (published.DistinctBy(r => r.Date).ToList(), failure);
        }

        private static List<ExchangeRateData> Usable(IEnumerable<ExchangeRateData> rates, DateOnly today) =>
            rates.Where(r => IsUsable(r, today)).ToList();

        private static bool IsUsable(ExchangeRateData rate, DateOnly today) =>
            rate.Date <= today && ExchangeRate.PublishedError(rate.Date, rate.Date, rate.BuyRate, rate.SellRate) is null;
    }
}
