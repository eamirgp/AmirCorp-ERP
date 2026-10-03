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
                options.AddPolicy(ExternalLookup, context => RateLimitPartition.GetFixedWindowLimiter(
                    context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonimo",
                    _ => new FixedWindowRateLimiterOptions { PermitLimit = 30, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
            });
    }
}
