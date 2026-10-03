using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace ERP.Api.Common
{
    /// <summary>
    /// Límites de pedidos por usuario. Las consultas a SUNAT, RENIEC y el tipo de cambio gastan el cupo del servicio
    /// externo (Decolecta: 1000 al mes): un error en la pantalla que repita la consulta, o alguien que insista, podría
    /// agotarlo. 30 por minuto sobra para quien trabaja normal.
    /// </summary>
    internal static class RateLimits
    {
        public const string ExternalLookup = "external-lookup";

        public static IServiceCollection AddApiRateLimits(this IServiceCollection services) =>
            services.AddRateLimiter(options =>
            {
                // Sin cuerpo: StatusCodeResponse le pone el mensaje en español.
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddPolicy(ExternalLookup, context =>
                    // El tipo de cambio "solo lo guardado" lee la base, no el servicio externo: no gasta el cupo (la
                    // pantalla lo pide en cada cambio de fecha y, si contara, el botón SUNAT respondería 429 sin motivo).
                    IsStoredOnly(context)
                        ? RateLimitPartition.GetNoLimiter("guardado")
                        : RateLimitPartition.GetFixedWindowLimiter(
                            context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonimo",
                            _ => new FixedWindowRateLimiterOptions { PermitLimit = 30, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
            });

        private static bool IsStoredOnly(HttpContext context) =>
            bool.TryParse(context.Request.Query["storedOnly"], out var storedOnly) && storedOnly;
    }
}
