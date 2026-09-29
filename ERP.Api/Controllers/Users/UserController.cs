using ERP.Api.Common;
using ERP.Application.Common.Responses;
using ERP.Api.Controllers.Users.Requests;
using ERP.Api.Extensions;
using ERP.Application.Features.Users.ActivateUser;
using ERP.Application.Features.Users.ChangeUserRole;
using ERP.Application.Features.Users.CreateUser;
using ERP.Application.Features.Users.DeactivateUser;
using ERP.Application.Features.Users.GetUser;
using ERP.Application.Features.Users.ListAssignableRoles;
using ERP.Application.Features.Users.ListUsers;
using ERP.Application.Features.Users.ResetUserPassword;
using ERP.Application.Features.Users.UpdateUserProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Users
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ApiController]
    [Route("api/users")]
    public sealed class UserController : ControllerBase
    {
        private readonly ICreateUserUseCase _createUserUseCase;
        private readonly IListAssignableRolesUseCase _listAssignableRolesUseCase;
        private readonly IListUsersUseCase _listUsersUseCase;
        private readonly IActivateUserUseCase _activateUserUseCase;
        private readonly IDeactivateUserUseCase _deactivateUserUseCase;
        private readonly IUpdateUserProfileUseCase _updateUserProfileUseCase;
        private readonly IChangeUserRoleUseCase _changeUserRoleUseCase;
        private readonly IResetUserPasswordUseCase _resetUserPasswordUseCase;
        private readonly IGetUserUseCase _getUserUseCase;

        public UserController(
            ICreateUserUseCase createUserUseCase,
            IListAssignableRolesUseCase listAssignableRolesUseCase,
            IListUsersUseCase listUsersUseCase,
            IActivateUserUseCase activateUserUseCase,
            IDeactivateUserUseCase deactivateUserUseCase,
            IUpdateUserProfileUseCase updateUserProfileUseCase,
            IChangeUserRoleUseCase changeUserRoleUseCase,
            IResetUserPasswordUseCase resetUserPasswordUseCase,
            IGetUserUseCase getUserUseCase
            )
        {
            _createUserUseCase = createUserUseCase;
            _listAssignableRolesUseCase = listAssignableRolesUseCase;
            _listUsersUseCase = listUsersUseCase;
            _activateUserUseCase = activateUserUseCase;
            _deactivateUserUseCase = deactivateUserUseCase;
            _updateUserProfileUseCase = updateUserProfileUseCase;
            _changeUserRoleUseCase = changeUserRoleUseCase;
            _resetUserPasswordUseCase = resetUserPasswordUseCase;
            _getUserUseCase = getUserUseCase;
        }

        [HttpPost]
        [ProducesResponseType<CreatedResponseDto>(StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody]CreateUserRequest createUserRequest)
        {
            var errors = createUserRequest.Validate();
            if (errors.Count > 0)
                return errors.ToBadRequest();

            var result = await _createUserUseCase.ExecuteAsync(createUserRequest.ToDto());
            return result.ToActionResult(StatusCodes.Status201Created);
        }

        [HttpGet("roles")]
        [ProducesResponseType<IReadOnlyCollection<ListAssignableRolesResponseDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListRoles() =>
            Ok(await _listAssignableRolesUseCase.ExecuteAsync());

        [HttpGet]
        [ProducesResponseType<IReadOnlyCollection<ListUsersResponseDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> List() =>
            Ok(await _listUsersUseCase.ExecuteAsync());

        [HttpPatch("{id:guid}/activate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Activate(Guid id)
        {
            var result = await _activateUserUseCase.ExecuteAsync(new ActivateUserDto(id));
            return result.ToActionResult(StatusCodes.Status204NoContent);
        }

        [HttpPatch("{id:guid}/deactivate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            var result = await _deactivateUserUseCase.ExecuteAsync(new DeactivateUserDto(id));
            return result.ToActionResult(StatusCodes.Status204NoContent);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateProfile(Guid id, [FromBody]UpdateUserProfileRequest updateUserProfileRequest)
        {
            var errors = updateUserProfileRequest.Validate();
            if (errors.Count > 0)
                return errors.ToBadRequest();

            var result = await _updateUserProfileUseCase.ExecuteAsync(updateUserProfileRequest.ToDto(id));
            return result.ToActionResult(StatusCodes.Status204NoContent);
        }

        [HttpPatch("{id:guid}/role")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> ChangeRole(Guid id, [FromBody]ChangeUserRoleRequest changeUserRoleRequest)
        {
            var errors = changeUserRoleRequest.Validate();
            if (errors.Count > 0)
                return errors.ToBadRequest();

            var result = await _changeUserRoleUseCase.ExecuteAsync(changeUserRoleRequest.ToDto(id));
            return result.ToActionResult(StatusCodes.Status204NoContent);
        }

        [HttpPatch("{id:guid}/password")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> ResetPassword(Guid id, [FromBody]ResetUserPasswordRequest resetUserPasswordRequest)
        {
            var errors = resetUserPasswordRequest.Validate();
            if (errors.Count > 0)
                return errors.ToBadRequest();

            var result = await _resetUserPasswordUseCase.ExecuteAsync(resetUserPasswordRequest.ToDto(id));
            return result.ToActionResult(StatusCodes.Status204NoContent);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType<GetUserResponseDto>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(Guid id)
        {
            var response = await _getUserUseCase.ExecuteAsync(new GetUserDto(id));
            return response is null ? NotFound(new ErrorResponse(["El usuario no existe."])) : Ok(response);
        }
    }
}
