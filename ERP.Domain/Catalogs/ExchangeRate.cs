using ERP.Domain.Common;

namespace ERP.Domain.Catalogs
{
    /// <summary>
    /// Tipo de cambio de una moneda (soles por unidad) que aplica a una fecha, guardado la primera vez que se consulta:
    /// el de una fecha ya publicada no cambia, así que no se vuelve a pedir al servicio externo.
    /// Si ese día no hubo publicación (fin de semana, feriado), aplica el último publicado y <see cref="PublishedDate"/>
    /// dice de qué día es.
    /// </summary>
    public sealed class ExchangeRate
    {
        public const int SourceMaxLength = 20;
        /// <summary>Cuántos días hacia atrás se busca el último publicado (feriados largos incluidos).</summary>
        public const int MaxDaysBack = 10;
        // Las columnas son numeric(18,6); un dólar nunca valdrá 1000 soles: más es un dato mal leído.
        public const decimal RateMax = 1000m;
        public const int RateDecimals = 6;
        private static readonly DateOnly MinDate = new(2000, 1, 1);

        public Currency Currency { get; private set; }
        /// <summary>Fecha a la que aplica.</summary>
        public DateOnly Date { get; private set; }
        /// <summary>Fecha de la publicación usada: la misma, o la última anterior si ese día no se publicó.</summary>
        public DateOnly PublishedDate { get; private set; }
        public decimal BuyRate { get; private set; }
        public decimal SellRate { get; private set; }
        /// <summary>Quién lo publica: "SUNAT".</summary>
        public string Source { get; private set; }
        public DateTime FetchedAt { get; private set; }

        private ExchangeRate(Currency currency, DateOnly date, DateOnly publishedDate, decimal buyRate, decimal sellRate, string source, DateTime fetchedAt)
        {
            Currency = currency;
            Date = date;
            PublishedDate = publishedDate;
            BuyRate = buyRate;
            SellRate = sellRate;
            Source = source;
            FetchedAt = fetchedAt;
        }

        public static ExchangeRate Create(Currency currency, DateOnly date, DateOnly publishedDate, decimal buyRate, decimal sellRate, string source, DateTime fetchedAt)
        {
            if (!Enum.IsDefined(currency) || currency is Currency.PEN)
                throw new DomainException("El tipo de cambio es de una moneda extranjera.");

            if (publishedDate > date)
                throw new DomainException("La publicación del tipo de cambio no puede ser posterior a la fecha a la que aplica.");

            if (date.DayNumber - publishedDate.DayNumber > MaxDaysBack)
                throw new DomainException($"El último tipo de cambio publicado no puede ser de más de {MaxDaysBack} días antes.");

            if ((RateError(buyRate) ?? RateError(sellRate)) is { } rateError)
                throw new DomainException(rateError);

            if (string.IsNullOrWhiteSpace(source) || source.Trim().Length > SourceMaxLength)
                throw new DomainException($"Quién publica el tipo de cambio es requerido, de hasta {SourceMaxLength} caracteres.");

            if (fetchedAt == default)
                throw new DomainException("La fecha de la consulta es requerida.");

            return new(currency, date, publishedDate, buyRate, sellRate, source.Trim(), fetchedAt);
        }

        /// <summary>Qué impide buscar el tipo de cambio de esa fecha, o null si se puede: no futura ni de un año mal escrito.</summary>
        public static string? LookupDateError(DateOnly date, DateOnly today) =>
            date > today ? "La fecha no puede ser mayor a la fecha actual."
            : date < MinDate ? "La fecha es demasiado antigua. Revisa el año."
            : null;

        /// <summary>
        /// El día más antiguo cuya publicación todavía aplica a esa fecha. Si ese día no se publicó, aplica el último
        /// publicado (Reglamento del IGV, art. 5, num. 17), buscando como máximo <see cref="MaxDaysBack"/> días hacia
        /// atrás, que cubre los feriados largos.
        /// </summary>
        public static DateOnly OldestApplicable(DateOnly date) =>
            date.AddDays(-MaxDaysBack);

        /// <summary>
        /// Si un día sin publicación se guarda con el último publicado, para no volver a consultarlo. Solo un día pasado:
        /// el de hoy puede publicarse más tarde.
        /// </summary>
        public static bool StoresDayWithoutPublication(DateOnly date, DateOnly publishedDate, DateOnly today) =>
            publishedDate < date && date < today;

        private static string? RateError(decimal rate) =>
            rate <= 0 ? "El tipo de cambio debe ser mayor a cero."
            : rate > RateMax || Math.Round(rate, RateDecimals) != rate ? $"El tipo de cambio debe ser menor a {RateMax} y tener hasta {RateDecimals} decimales."
            : null;
    }
}
