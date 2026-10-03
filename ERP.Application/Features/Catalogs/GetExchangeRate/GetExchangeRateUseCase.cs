using System.Globalization;
using ERP.Application.Common.Results;
using ERP.Application.Contracts.Infrastructure;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.Catalogs;
using ERP.Domain.Common;

namespace ERP.Application.Features.Catalogs.GetExchangeRate
{
    /// <summary>Tipo de cambio para llenar una compra, con el texto que explica de dónde salió.</summary>
    public sealed record GetExchangeRateResponseDto(
        decimal Rate,
        // Fecha de lo publicado: anterior a la pedida si ese día no hubo publicación.
        DateOnly Date,
        string Source,
        // "Tipo de cambio venta de SUNAT del 02/10/2026."
        string Description
        );

    public interface IGetExchangeRateUseCase
    {
        /// <param name="storedOnly">
        /// Solo lo que ya está guardado, sin consultar al servicio externo: la pantalla lo usa para llenar el campo sola
        /// al elegir la fecha, sin gastar consultas. Si no está guardado responde "no encontrado" y lo pide el usuario.
        /// </param>
        Task<Result<GetExchangeRateResponseDto>> ExecuteAsync(Currency currency, DateOnly date, bool storedOnly = false, CancellationToken ct = default);
    }

    /// <summary>
    /// Tipo de cambio de SUNAT para una moneda y una fecha. Para compras se usa el de venta: es el que fija el
    /// Reglamento del IGV (art. 5, num. 17) para operaciones en moneda extranjera, en la fecha en que nace la
    /// obligación; si ese día no se publicó, el último publicado.
    /// Primero se busca en la base de datos: el de una fecha ya publicada no cambia. Solo si falta se consulta al
    /// servicio externo (el mes completo, si lo ofrece) y se guarda para no volver a pedirlo.
    /// </summary>
    internal sealed class GetExchangeRateUseCase : IGetExchangeRateUseCase
    {
        private const string Source = "SUNAT";

        private readonly IExchangeRateLookup _exchangeRateLookup;
        private readonly IExchangeRateRepository _exchangeRateRepository;
        private readonly IUnitOfWork _unitOfWork;

        public GetExchangeRateUseCase(IExchangeRateLookup exchangeRateLookup, IExchangeRateRepository exchangeRateRepository, IUnitOfWork unitOfWork)
        {
            _exchangeRateLookup = exchangeRateLookup;
            _exchangeRateRepository = exchangeRateRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<GetExchangeRateResponseDto>> ExecuteAsync(Currency currency, DateOnly date, bool storedOnly = false, CancellationToken ct = default)
        {
            // SUNAT publica el tipo de cambio del dólar; es la única moneda extranjera del sistema.
            if (currency is not Currency.USD)
                return Failure(["Solo hay tipo de cambio para dólares."], ErrorType.BadRequest);

            var now = DateTime.UtcNow;
            var today = PeruCalendar.Today(now);
            if (ExchangeRate.LookupDateError(date, today) is { } dateError)
                return Failure([dateError], ErrorType.BadRequest);

            if (await _exchangeRateRepository.GetAsync(currency, date) is { } stored)
                return Success(stored.SellRate, stored.PublishedDate, date);

            if (storedOnly)
                return Failure([$"El tipo de cambio del {Text(date)} todavía no está guardado."], ErrorType.NotFound);

            if (!_exchangeRateLookup.IsConfigured)
                return Failure([$"La consulta del tipo de cambio en {Source} no está configurada. Escríbelo a mano."], ErrorType.Unavailable);

            var notBefore = ExchangeRate.OldestApplicable(date);
            // Lo que ya está guardado en los meses consultados (el de la fecha y, si hace falta, el anterior), para no
            // guardarlo dos veces.
            var monthStart = new DateOnly(date.Year, date.Month, 1);
            var previousMonthStart = new DateOnly(notBefore.Year, notBefore.Month, 1);
            var known = (await _exchangeRateRepository.ExistingDatesAsync(
                currency, previousMonthStart, monthStart.AddMonths(1).AddDays(-1))).ToHashSet();

            // 1) El mes completo en una consulta: lo publicado que falte se guarda de una vez. Si la fecha es de los
            // primeros días y el mes todavía no trae nada hasta ella (1 de enero, feriados), también el mes anterior.
            var published = (await _exchangeRateLookup.FindMonthAsync(date.Year, date.Month, ct)).ToList();
            if (previousMonthStart < monthStart && published.All(r => r.Date > date))
                published.AddRange(await _exchangeRateLookup.FindMonthAsync(previousMonthStart.Year, previousMonthStart.Month, ct));
            published.RemoveAll(r => r.Date > today);

            // 2) Si los meses no trajeron nada de esa fecha ni de antes (o el proveedor no ofrece el mes), se pide la fecha.
            RucLookupFailure? failure = null;
            if (published.All(r => r.Date > date))
            {
                var single = await _exchangeRateLookup.FindAsync(date, ct);
                if (single.Data is { } data && data.Date <= date)
                    published.Add(data);
                else
                    failure = single.Failure ?? RucLookupFailure.NotFound;
            }

            foreach (var rate in published.DistinctBy(r => r.Date).Where(r => known.Add(r.Date)))
                _exchangeRateRepository.Add(ExchangeRate.Create(currency, rate.Date, rate.Date, rate.BuyRate, rate.SellRate, Source, now));

            // 3) El que aplica: el de la fecha o, si ese día no se publicó, el último anterior (lo recién traído o lo guardado).
            // Si el servicio no respondió, no se usa lo guardado: ese día pudo tener una publicación que todavía no se conoce.
            var providerFailed = failure is RucLookupFailure.Unauthorized or RucLookupFailure.Unavailable or RucLookupFailure.QuotaExceeded;
            var fromProvider = published.Where(r => r.Date <= date && r.Date >= notBefore).OrderByDescending(r => r.Date).FirstOrDefault();
            var fromStore = providerFailed ? null : await _exchangeRateRepository.LatestOnOrBeforeAsync(currency, date, notBefore);

            var best = fromStore is not null && (fromProvider is null || fromStore.PublishedDate > fromProvider.Date)
                ? new ExchangeRateData(fromStore.PublishedDate, fromStore.BuyRate, fromStore.SellRate)
                : fromProvider;

            if (best is null)
            {
                await _unitOfWork.SaveChangesAsync();

                return failure switch
                {
                    RucLookupFailure.Unauthorized => Failure(
                        ["El servicio de consulta rechazó la clave: puede haber vencido. Avisa al administrador del sistema y, mientras tanto, escribe el tipo de cambio a mano."],
                        ErrorType.Unavailable),
                    RucLookupFailure.Unavailable => Failure(
                        [$"No se pudo consultar {Source} en este momento. Inténtalo en unos minutos o escribe el tipo de cambio a mano."], ErrorType.Unavailable),
                    RucLookupFailure.QuotaExceeded => Failure(
                        [$"Se acabaron las consultas a {Source} de este mes. Escribe el tipo de cambio a mano; las consultas vuelven el próximo mes."], ErrorType.Unavailable),
                    _ => Failure([$"{Source} no tiene tipo de cambio publicado para el {Text(date)}. Escríbelo a mano."], ErrorType.NotFound),
                };
            }

            // Un día pasado sin publicación ya no la tendrá: se guarda con el último publicado para no volver a
            // consultarlo (la regla de cuándo, en el dominio). Solo si lo confirmó el servicio (lo que trajo es lo que
            // aplica), no con lo guardado solamente.
            if (ExchangeRate.StoresDayWithoutPublication(date, best.Date, today) && best == fromProvider && known.Add(date))
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
