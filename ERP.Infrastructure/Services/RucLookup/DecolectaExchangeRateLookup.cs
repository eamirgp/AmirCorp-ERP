using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using ERP.Application.Contracts.Infrastructure;
using Microsoft.Extensions.Options;

namespace ERP.Infrastructure.Services.RucLookup
{
    /// <summary>
    /// Tipo de cambio de SUNAT con Decolecta, con el token como Bearer:
    /// GET /v1/tipo-cambio/sunat?date=AAAA-MM-DD (una fecha) o ?month=M&amp;year=AAAA (un mes).
    /// Documentación: https://decolecta.gitbook.io/docs/servicios/integrations
    /// No guarda nada: lo consultado lo guarda quien llama, en la base de datos.
    /// </summary>
    internal sealed class DecolectaExchangeRateLookup : IExchangeRateLookup
    {
        private static readonly HttpClient Http = new(new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(10) })
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        // La documentación no dice si la consulta por mes devuelve la lista de días. Si responde con un solo día, no
        // sirve para ahorrar consultas y no se vuelve a intentar mientras la API esté encendida.
        private static volatile bool _monthQueryUseless;

        private readonly RucLookupSettings _settings;

        public DecolectaExchangeRateLookup(IOptions<RucLookupSettings> settings) => _settings = settings.Value;

        public bool IsConfigured => !string.IsNullOrWhiteSpace(_settings.Token);

        public async Task<ExchangeRateOutcome> FindAsync(DateOnly date, CancellationToken ct = default)
        {
            if (!IsConfigured)
                return ExchangeRateOutcome.Failed(RucLookupFailure.Unauthorized);

            try
            {
                using var response = await GetAsync($"date={date:yyyy-MM-dd}", ct);

                if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                    return ExchangeRateOutcome.Failed(RucLookupFailure.Unauthorized);

                if (response.StatusCode is HttpStatusCode.TooManyRequests)
                    return ExchangeRateOutcome.Failed(RucLookupFailure.QuotaExceeded);

                if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound or HttpStatusCode.UnprocessableEntity)
                    return ExchangeRateOutcome.Failed(RucLookupFailure.NotFound);

                if (!response.IsSuccessStatusCode)
                    return ExchangeRateOutcome.Failed(RucLookupFailure.Unavailable);

                using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
                var rates = Parse(json.RootElement);

                // Lo publicado puede ser de una fecha anterior a la pedida (fin de semana, feriado): se informa la que vino.
                return rates.Count > 0
                    ? ExchangeRateOutcome.Found(rates.OrderByDescending(r => r.Date).First())
                    : ExchangeRateOutcome.Failed(RucLookupFailure.NotFound);
            }
            catch (Exception ex) when ((ex is HttpRequestException or TaskCanceledException or JsonException) && !ct.IsCancellationRequested)
            {
                return ExchangeRateOutcome.Failed(RucLookupFailure.Unavailable);
            }
        }

        public async Task<IReadOnlyCollection<ExchangeRateData>> FindMonthAsync(int year, int month, CancellationToken ct = default)
        {
            if (!IsConfigured || _monthQueryUseless)
                return [];

            try
            {
                using var response = await GetAsync($"month={month}&year={year}", ct);
                if (!response.IsSuccessStatusCode)
                    return [];

                using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
                if (json.RootElement.ValueKind != JsonValueKind.Array)
                {
                    _monthQueryUseless = true;
                    return [];
                }

                return Parse(json.RootElement).Where(r => r.Date.Year == year && r.Date.Month == month).ToArray();
            }
            catch (Exception ex) when ((ex is HttpRequestException or TaskCanceledException or JsonException) && !ct.IsCancellationRequested)
            {
                return [];
            }
        }

        private async Task<HttpResponseMessage> GetAsync(string query, CancellationToken ct)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{_settings.BaseUrl.TrimEnd('/')}/v1/tipo-cambio/sunat?{query}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.Token);
            return await Http.SendAsync(request, ct);
        }

        /// <summary>Un día o una lista de días: { buy_price, sell_price, date }, con los montos como texto ("3.552").</summary>
        private static List<ExchangeRateData> Parse(JsonElement root)
        {
            var items = root.ValueKind == JsonValueKind.Array ? root.EnumerateArray().ToList() : [root];
            var rates = new List<ExchangeRateData>();

            foreach (var item in items)
            {
                if (item.ValueKind != JsonValueKind.Object)
                    continue;

                if (Number(item, "buy_price") is { } buy && Number(item, "sell_price") is { } sell && buy > 0 && sell > 0
                    && item.TryGetProperty("date", out var date)
                    && DateOnly.TryParseExact(date.GetString(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var day))
                    rates.Add(new ExchangeRateData(day, buy, sell));
            }

            return rates;
        }

        private static decimal? Number(JsonElement item, string name)
        {
            if (!item.TryGetProperty(name, out var value))
                return null;

            return value.ValueKind switch
            {
                JsonValueKind.String when decimal.TryParse(value.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var d) => d,
                JsonValueKind.Number when value.TryGetDecimal(out var d) => d,
                _ => null
            };
        }
    }
}
