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

            if (buyRate <= 0 || sellRate <= 0)
                throw new DomainException("El tipo de cambio debe ser mayor a cero.");

            return new(currency, date, publishedDate, buyRate, sellRate, source, fetchedAt);
        }
    }
}
