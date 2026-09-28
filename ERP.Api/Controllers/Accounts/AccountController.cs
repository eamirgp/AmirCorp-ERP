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
            Ok(await _getMyProfileUseCase.ExecuteAsync());
    }
}
