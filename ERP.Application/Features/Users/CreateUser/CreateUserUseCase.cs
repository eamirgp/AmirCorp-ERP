using ERP.Application.Common.Responses;
using ERP.Application.Common.Results;
using ERP.Application.Contracts.Api;
using ERP.Application.Contracts.Infrastructure;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.Users;
using ERP.Domain.Users.Enums;

namespace ERP.Application.Features.Users.CreateUser
{
    internal sealed class CreateUserUseCase : ICreateUserUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public CreateUserUseCase(
            IUserRepository userRepository,
            IPasswordService passwordService,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork
            )
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<CreatedResponseDto>> ExecuteAsync(CreateUserDto request)
        {
            // La misma regla del dominio, revisada antes para responder 403 con el mensaje.
            if (User.AssignRoleError(_currentUser.Role, request.Role) is { } roleError)
                return Result<CreatedResponseDto>.Failure([roleError], ErrorType.Forbidden);

            if (await _userRepository.FindByEmailAsync(request.Email) is { } owner)
                return Result<CreatedResponseDto>.Failure([User.EmailTakenError(owner)], ErrorType.Conflict);

            var user = User.Create(
                request.Name,
                request.Email,
                request.Password,
                _passwordService.Hash,
                request.Role,
                _currentUser.Role
                );

            _userRepository.Add(user);

            await _unitOfWork.SaveChangesAsync();

            return Result<CreatedResponseDto>.Success(new CreatedResponseDto(user.Id));
        }
    }
}
