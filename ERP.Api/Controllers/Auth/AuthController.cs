using ERP.Api.Controllers.Auth.Requests;
using ERP.Api.Extensions;
using ERP.Application.Common.Results;
using ERP.Application.Features.Auth;
using ERP.Application.Features.Auth.Login;
using ERP.Application.Features.Auth.Logout;
using ERP.Application.Features.Auth.Refresh;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Auth
{
    /// <summary>
    /// Sesión con refresh token (decisión 24): el token de acceso (15 minutos) va en la respuesta y la pantalla lo guarda
    /// solo en memoria; el refresh token va en una cookie httpOnly, que JavaScript no puede leer, limitada a /api/auth.
    /// </summary>
    [AllowAnonymous]
    [ApiController]
    [Route("api/auth")]
    public sealed class AuthController : ControllerBase
    {
        private const string RefreshCookie = "erp_refresh";
        private const string RefreshCookiePath = "/api/auth";

        private readonly ILoginUseCase _loginUseCase;
        private readonly IRefreshSessionUseCase _refreshSessionUseCase;
        private readonly ILogoutUseCase _logoutUseCase;

        public AuthController(ILoginUseCase loginUseCase, IRefreshSessionUseCase refreshSessionUseCase, ILogoutUseCase logoutUseCase)
        {
            _loginUseCase = loginUseCase;
            _refreshSessionUseCase = refreshSessionUseCase;
            _logoutUseCase = logoutUseCase;
        }

        [HttpPost("login")]
        [ProducesResponseType<LoginResponseDto>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Login([FromBody]LoginRequest loginRequest)
        {
            var errors = loginRequest.Validate();
            if (errors.Count > 0)
                return errors.ToBadRequest();

            // La dirección del equipo sirve para contar los intentos fallidos desde ahí, además de los de ese correo.
            var result = await _loginUseCase.ExecuteAsync(loginRequest.ToDto(HttpContext.Connection.RemoteIpAddress?.ToString()));
            return SessionResponse(result);
        }

        /// <summary>
        /// Renueva la sesión con la cookie: entrega un token de acceso nuevo y cambia el refresh token. La pantalla lo
        /// llama al abrirse (para recuperar la sesión) y cuando su token de acceso está por vencer.
        /// </summary>
        [HttpPost("refresh")]
        [ProducesResponseType<LoginResponseDto>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Refresh()
        {
            var result = await _refreshSessionUseCase.ExecuteAsync(Request.Cookies[RefreshCookie]);
            if (!result.IsSuccess)
                DeleteRefreshCookie();

            return SessionResponse(result);
        }

        /// <summary>Cierra la sesión en el servidor: el refresh token deja de servir y se borra la cookie.</summary>
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Logout()
        {
            await _logoutUseCase.ExecuteAsync(Request.Cookies[RefreshCookie]);
            DeleteRefreshCookie();
            return NoContent();
        }

        private IActionResult SessionResponse(Result<AuthSessionDto> result)
        {
            if (!result.IsSuccess)
                return Result<LoginResponseDto>.Failure(result.Errors, result.ErrorType!.Value).ToActionResult(StatusCodes.Status200OK);

            var session = result.Value!;
            if (session.RefreshToken is { } refreshToken)
                Response.Cookies.Append(RefreshCookie, refreshToken, CookieOptions(session.SessionExpiresAt));

            return Ok(new LoginResponseDto(session.AccessToken, session.SessionExpiresAt));
        }

        private void DeleteRefreshCookie() =>
            Response.Cookies.Delete(RefreshCookie, CookieOptions(expires: null));

        // httpOnly: JavaScript no la lee. Secure con HTTPS (en producción siempre). SameSite=Strict: no viaja en pedidos
        // que inicien otros sitios. Solo va a /api/auth: los demás pedidos no la llevan.
        private CookieOptions CookieOptions(DateTime? expires) => new()
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Path = RefreshCookiePath,
            Expires = expires is { } value ? new DateTimeOffset(value, TimeSpan.Zero) : null,
            IsEssential = true
        };
    }
}
