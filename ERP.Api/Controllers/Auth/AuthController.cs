using ERP.Api.Controllers.Auth.Requests;
using ERP.Api.Extensions;
using ERP.Application.Features.Auth.Login;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Auth
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/auth")]
    public sealed class AuthController : ControllerBase
    {
        private readonly ILoginUseCase _loginUseCase;

        public AuthController(ILoginUseCase loginUseCase)
        {
            _loginUseCase = loginUseCase;
        }

        [HttpPost("login")]
        [ProducesResponseType<LoginResponseDto>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Login([FromBody]LoginRequest loginRequest)
        {
            var errors = loginRequest.Validate();
            if (errors.Count > 0)
                return errors.ToBadRequest();

            var result = await _loginUseCase.ExecuteAsync(loginRequest.ToDto());
            return result.ToActionResult(StatusCodes.Status200OK);
        }
    }
}
