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
            if (!_currentUser.Role.CanManage(request.Role))
                return Result<CreatedResponseDto>.Failure(["No tienes permisos para asignar este rol."], ErrorType.Forbidden);

            if (await _userRepository.EmailExistsAsync(request.Email))
                return Result<CreatedResponseDto>.Failure(["El correo ya se encuentra en uso."], ErrorType.Conflict);

            var user = User.Create(
                request.Name,
                request.Email,
                _passwordService.Hash(request.Password),
                request.Role
                );

            _userRepository.Add(user);

            await _unitOfWork.SaveChangesAsync();

            return Result<CreatedResponseDto>.Success(new CreatedResponseDto(user.Id));
        }
    }
}
