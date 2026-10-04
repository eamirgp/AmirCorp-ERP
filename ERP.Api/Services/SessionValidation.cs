using ERP.Api.Common;
using ERP.Application.Contracts.Infrastructure;
using ERP.Application.Features.Auth.Session;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

namespace ERP.Api.Services
{
    /// <summary>
    /// Completa la validación del token: además de la firma y el vencimiento, en cada pedido revisa que el usuario siga
    /// activo y reemplaza el rol del token por el actual (<see cref="IValidateSessionUseCase"/>). Si ya no puede entrar,
    /// responde 401 con el motivo para que la pantalla lo muestre al volver al inicio de sesión.
    /// </summary>
    internal static class SessionValidation
    {
        private const string FailureKey = "SessionFailure";

        public static JwtBearerEvents Events() => new()
        {
            OnTokenValidated = async context =>
            {
                if (context.Principal?.Identity is not ClaimsIdentity identity
                    || !Guid.TryParse(identity.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId)
                    || !Guid.TryParse(identity.FindFirst(IJwtService.SessionClaim)?.Value, out var sessionId))
                {
                    // Un token de antes de este cambio no dice su sesión: la pantalla lo renueva y sigue.
                    context.Fail("El token no tiene el usuario o la sesión.");
                    return;
                }

                var useCase = context.HttpContext.RequestServices.GetRequiredService<IValidateSessionUseCase>();
                var result = await useCase.ExecuteAsync(userId, sessionId);

                if (!result.IsSuccess)
                {
                    var message = result.Errors.First();
                    context.HttpContext.Items[FailureKey] = message;
                    context.Fail(message);
                    return;
                }

                // El rol de hoy, no el que tenía al iniciar sesión: los permisos ([Authorize(Roles)] y los casos de uso) lo usan.
                foreach (var claim in identity.FindAll(identity.RoleClaimType).ToList())
                    identity.RemoveClaim(claim);
                identity.AddClaim(new Claim(identity.RoleClaimType, result.Value.ToString()));
            },

            OnChallenge = async context =>
            {
                if (context.HttpContext.Items[FailureKey] is not string message)
                    return;

                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(ErrorResponse.From([message]));
            }
        };
    }
}
