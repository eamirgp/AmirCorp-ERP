namespace ERP.Application.Contracts.Infrastructure
{
    /// <summary>
    /// Tipo de cambio del dólar que publica SUNAT para una fecha, a través de un proveedor externo (hoy Decolecta, con
    /// la misma clave de la consulta de RUC). Sin clave configurada no está disponible y el tipo de cambio se escribe a mano.
    /// </summary>
    public interface IExchangeRateLookup
    {
        bool IsConfigured { get; }

        Task<ExchangeRateOutcome> FindAsync(DateOnly date, CancellationToken ct = default);

        /// <summary>
        /// Todo lo publicado en un mes, en una sola consulta. Devuelve una lista vacía si el proveedor no lo ofrece o
        /// falla: quien llama sigue con la consulta por fecha.
        /// </summary>
        Task<IReadOnlyCollection<ExchangeRateData>> FindMonthAsync(int year, int month, CancellationToken ct = default);
    }

    /// <summary>Soles por dólar, compra y venta, y la fecha a la que corresponde lo publicado.</summary>
    public sealed record ExchangeRateData(DateOnly Date, decimal BuyRate, decimal SellRate);

    /// <summary>Resultado de la consulta: los datos o el motivo por el que no se obtuvieron.</summary>
    public sealed record ExchangeRateOutcome(ExchangeRateData? Data, RucLookupFailure? Failure)
    {
        public static ExchangeRateOutcome Found(ExchangeRateData data) => new(data, null);
        public static ExchangeRateOutcome Failed(RucLookupFailure failure) => new(null, failure);
    }
}
