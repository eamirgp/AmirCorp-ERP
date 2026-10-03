using ERP.Api.Common;
using ERP.Application.Features.Accounts.GetMyProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Accounts
{
    [Authorize]
    [ApiController]
    [Route("api/me")]
    public sealed class AccountController : ControllerBase
    {
        private readonly IGetMyProfileUseCase _getMyProfileUseCase;

        public AccountController(IGetMyProfileUseCase getMyProfileUseCase)
        {
            _getMyProfileUseCase = getMyProfileUseCase;
        }

        [HttpGet]
        [ProducesResponseType<GetMyProfileResponseDto>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Get() =>
            await _getMyProfileUseCase.ExecuteAsync() is { } profile
                ? Ok(profile)
                : NotFound(new ErrorResponse(["Tu usuario ya no existe."]));
    }
}
