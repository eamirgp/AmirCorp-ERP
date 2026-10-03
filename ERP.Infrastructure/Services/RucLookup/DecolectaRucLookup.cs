using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using ERP.Application.Contracts.Infrastructure;
using Microsoft.Extensions.Options;

namespace ERP.Infrastructure.Services.RucLookup
{
    /// <summary>Configuración de la consulta de RUC. El token va en User Secrets o variables de entorno, nunca en appsettings.</summary>
    public sealed class RucLookupSettings
    {
        public string BaseUrl { get; set; } = "https://api.decolecta.com";
        public string? Token { get; set; }
    }

    /// <summary>
    /// Consulta de RUC (SUNAT) y de DNI (RENIEC) con Decolecta (antes apis.net.pe), con el token como Bearer.
    /// RUC: GET /v1/sunat/ruc?numero=… Documentación: https://decolecta.gitbook.io/docs/servicios/integrations
    /// DNI: GET /v1/reniec/dni?numero=… Documentación: https://decolecta.gitbook.io/docs/servicios/integrations-2
    /// </summary>
    internal sealed class DecolectaRucLookup : IRucLookup
    {
        // Un solo cliente para toda la aplicación, renovando las conexiones cada tanto (buena práctica de HttpClient).
        private static readonly HttpClient Http = new(new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(10) })
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        private readonly RucLookupSettings _settings;

        public DecolectaRucLookup(IOptions<RucLookupSettings> settings) => _settings = settings.Value;

        public bool IsConfigured => !string.IsNullOrWhiteSpace(_settings.Token);

        public async Task<RucLookupOutcome> FindAsync(string ruc, CancellationToken ct = default)
        {
            if (!IsConfigured)
                return RucLookupOutcome.Failed(LookupFailure.Unauthorized);

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, $"{_settings.BaseUrl.TrimEnd('/')}/v1/sunat/ruc?numero={Uri.EscapeDataString(ruc)}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.Token);

                using var response = await Http.SendAsync(request, ct);

                if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                    return RucLookupOutcome.Failed(LookupFailure.Unauthorized);

                if (response.StatusCode is HttpStatusCode.TooManyRequests)
                    return RucLookupOutcome.Failed(LookupFailure.QuotaExceeded);

                // Decolecta responde 422 (o 400) cuando el RUC no existe o no es válido.
                if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound or HttpStatusCode.UnprocessableEntity)
                    return RucLookupOutcome.Failed(LookupFailure.NotFound);

                if (!response.IsSuccessStatusCode)
                    return RucLookupOutcome.Failed(LookupFailure.Unavailable);

                var body = await response.Content.ReadFromJsonAsync<DecolectaRuc>(ct);
                if (body is null || string.IsNullOrWhiteSpace(body.RazonSocial))
                    return RucLookupOutcome.Failed(LookupFailure.NotFound);

                return RucLookupOutcome.Found(new RucLookupData(
                    body.NumeroDocumento ?? ruc,
                    body.RazonSocial.Trim(),
                    body.Estado?.Trim() ?? "",
                    body.Condicion?.Trim() ?? "",
                    Address(body)
                    ));
            }
            catch (Exception ex) when ((ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException or NotSupportedException) && !ct.IsCancellationRequested)
            {
                return RucLookupOutcome.Failed(LookupFailure.Unavailable);
            }
        }

        /// <summary>DNI en RENIEC: GET /v1/reniec/dni?numero=… Decolecta responde 400 cuando el DNI no existe.</summary>
        public async Task<DniLookupOutcome> FindDniAsync(string dni, CancellationToken ct = default)
        {
            if (!IsConfigured)
                return DniLookupOutcome.Failed(LookupFailure.Unauthorized);

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, $"{_settings.BaseUrl.TrimEnd('/')}/v1/reniec/dni?numero={Uri.EscapeDataString(dni)}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.Token);

                using var response = await Http.SendAsync(request, ct);

                if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                    return DniLookupOutcome.Failed(LookupFailure.Unauthorized);

                if (response.StatusCode is HttpStatusCode.TooManyRequests)
                    return DniLookupOutcome.Failed(LookupFailure.QuotaExceeded);

                if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound or HttpStatusCode.UnprocessableEntity)
                    return DniLookupOutcome.Failed(LookupFailure.NotFound);

                if (!response.IsSuccessStatusCode)
                    return DniLookupOutcome.Failed(LookupFailure.Unavailable);

                var body = await response.Content.ReadFromJsonAsync<DecolectaDni>(ct);
                if (body is null || string.IsNullOrWhiteSpace(body.FullName))
                    return DniLookupOutcome.Failed(LookupFailure.NotFound);

                return DniLookupOutcome.Found(body.FullName.Trim());
            }
            catch (Exception ex) when ((ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException or NotSupportedException) && !ct.IsCancellationRequested)
            {
                return DniLookupOutcome.Failed(LookupFailure.Unavailable);
            }
        }

        // Apellidos y nombres, en el mismo orden que usa SUNAT para las personas con RUC.
        private sealed record DecolectaDni([property: JsonPropertyName("full_name")] string? FullName);

        /// <summary>"AV. LOS OLIVOS 123, SAN ISIDRO - LIMA - LIMA", o null si SUNAT no tiene dirección ("-").</summary>
        private static string? Address(DecolectaRuc body)
        {
            var street = body.Direccion?.Trim();
            if (string.IsNullOrWhiteSpace(street) || street == "-")
                return null;

            var place = string.Join(" - ", new[] { body.Distrito, body.Provincia, body.Departamento }
                .Where(p => !string.IsNullOrWhiteSpace(p) && p != "-")
                .Select(p => p!.Trim()));

            return place.Length > 0 && !street.Contains(place, StringComparison.OrdinalIgnoreCase) ? $"{street}, {place}" : street;
        }

        private sealed record DecolectaRuc(
            [property: JsonPropertyName("razon_social")] string? RazonSocial,
            [property: JsonPropertyName("numero_documento")] string? NumeroDocumento,
            [property: JsonPropertyName("estado")] string? Estado,
            [property: JsonPropertyName("condicion")] string? Condicion,
            [property: JsonPropertyName("direccion")] string? Direccion,
            [property: JsonPropertyName("distrito")] string? Distrito,
            [property: JsonPropertyName("provincia")] string? Provincia,
            [property: JsonPropertyName("departamento")] string? Departamento
            );
    }
}
