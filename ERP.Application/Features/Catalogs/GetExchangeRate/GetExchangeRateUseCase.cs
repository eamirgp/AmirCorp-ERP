using System.Globalization;
using ERP.Application.Common.Lookup;
using ERP.Application.Common.Results;
using ERP.Application.Contracts.Infrastructure;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.Catalogs;
using ERP.Domain.Common;

namespace ERP.Application.Features.Catalogs.GetExchangeRate
{
    /// <summary>
    /// Tipo de cambio de SUNAT para una moneda y una fecha. Para compras se usa el de venta: es el que fija el
    /// Reglamento del IGV (art. 5, num. 17) para operaciones en moneda extranjera, en la fecha en que nace la
    /// obligación; si ese día no se publicó, el último publicado.
    /// Primero se busca en la base de datos: el de una fecha ya publicada no cambia. Solo si falta se consulta al
    /// servicio externo (<see cref="ExchangeRateFetcher"/>) y se guarda para no volver a pedirlo. Las reglas (qué fecha
    /// se puede pedir, cuál aplica, cuándo se guarda un día sin publicación) son del dominio (<see cref="ExchangeRate"/>).
    /// </summary>
    internal sealed class GetExchangeRateUseCase : IGetExchangeRateUseCase
    {
        private const string Source = "SUNAT";

        private readonly IExchangeRateLookup _exchangeRateLookup;
        private readonly ExchangeRateFetcher _fetcher;
        private readonly IExchangeRateRepository _exchangeRateRepository;
        private readonly TimeProvider _timeProvider;
        private readonly IUnitOfWork _unitOfWork;

        public GetExchangeRateUseCase(
            IExchangeRateLookup exchangeRateLookup,
            ExchangeRateFetcher fetcher,
            IExchangeRateRepository exchangeRateRepository,
            TimeProvider timeProvider,
            IUnitOfWork unitOfWork)
        {
            _exchangeRateLookup = exchangeRateLookup;
            _fetcher = fetcher;
            _exchangeRateRepository = exchangeRateRepository;
            _timeProvider = timeProvider;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<GetExchangeRateResponseDto>> ExecuteAsync(Currency currency, DateOnly date, bool storedOnly = false, CancellationToken ct = default)
        {
            if (!currency.HasPublishedExchangeRate)
                return Failure(["Solo hay tipo de cambio para dólares."], ErrorType.BadRequest);

            var now = _timeProvider.GetUtcNow().UtcDateTime;
            var today = PeruCalendar.Today(now);
            if (ExchangeRate.LookupDateError(date, today) is { } dateError)
                return Failure([dateError], ErrorType.BadRequest);

            if (await _exchangeRateRepository.GetAsync(currency, date) is { } stored)
                return Success(stored.SellRate, stored.PublishedDate, date);

            if (storedOnly)
                return Failure([$"El tipo de cambio del {Text(date)} todavía no está guardado."], ErrorType.NotFound);

            if (!_exchangeRateLookup.IsConfigured)
                return Failure([$"La consulta del tipo de cambio en {Source} no está configurada. Escríbelo a mano."], ErrorType.Unavailable);

            // Lo publicado que falte se guarda de una vez (lo ya guardado no se repite).
            var (from, to) = ExchangeRateFetcher.CoveredRange(date);
            var known = (await _exchangeRateRepository.ExistingDatesAsync(currency, from, to)).ToHashSet();
            var (published, failure) = await _fetcher.FetchAsync(date, today, ct);
            foreach (var rate in published.Where(r => known.Add(r.Date)))
                _exchangeRateRepository.Add(ExchangeRate.Create(currency, rate.Date, rate.Date, rate.BuyRate, rate.SellRate, Source, now));

            // El que aplica, entre lo recién traído y lo guardado. Si el servicio no respondió, no se usa lo guardado:
            // ese día pudo tener una publicación que todavía no se conoce.
            var providerAnswered = failure is null or LookupFailure.NotFound;
            var fromStore = providerAnswered ? await _exchangeRateRepository.LatestOnOrBeforeAsync(currency, date, ExchangeRate.OldestApplicable(date)) : null;
            var candidates = published.Select(r => (r.Date, r.BuyRate, r.SellRate, FromProvider: true)).ToList();
            if (fromStore is not null)
                candidates.Add((fromStore.PublishedDate, fromStore.BuyRate, fromStore.SellRate, FromProvider: false));

            var applicable = ExchangeRate.ApplicablePublication(date, candidates.Select(c => c.Date));
            if (applicable is null)
            {
                await _unitOfWork.SaveChangesAsync();
                return LookupFailureMessage.For(failure ?? LookupFailure.NotFound, Source, "el tipo de cambio") is { } problem
                    ? Failure([problem.Message], problem.Type)
                    : Failure([$"{Source} no tiene tipo de cambio publicado para el {Text(date)}. Escríbelo a mano."], ErrorType.NotFound);
            }

            // A igual fecha, lo que trajo el servicio.
            var best = candidates.Where(c => c.Date == applicable).OrderByDescending(c => c.FromProvider).First();

            // Un día pasado sin publicación se guarda con el último publicado, solo si lo confirmó el servicio.
            if (best.FromProvider && ExchangeRate.StoresDayWithoutPublication(date, best.Date, today) && known.Add(date))
                _exchangeRateRepository.Add(ExchangeRate.Create(currency, date, best.Date, best.BuyRate, best.SellRate, Source, now));

            await _unitOfWork.SaveChangesAsync();

            return Success(best.SellRate, best.Date, date);
        }

        private static Result<GetExchangeRateResponseDto> Success(decimal rate, DateOnly publishedDate, DateOnly requestedDate)
        {
            var description = publishedDate == requestedDate
                ? $"Tipo de cambio venta de {Source} del {Text(publishedDate)}."
                : $"Tipo de cambio venta de {Source} del {Text(publishedDate)}, el último publicado antes del {Text(requestedDate)}.";

            return Result<GetExchangeRateResponseDto>.Success(new GetExchangeRateResponseDto(rate, publishedDate, Source, description));
        }

        private static string Text(DateOnly date) => date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

        private static Result<GetExchangeRateResponseDto> Failure(string[] errors, ErrorType type) =>
            Result<GetExchangeRateResponseDto>.Failure(errors, type);
    }
}
