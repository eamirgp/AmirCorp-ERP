using ERP.Application.Common.Results;
using ERP.Application.Contracts.Api;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.Users.Enums;

namespace ERP.Application.Features.Users.ActivateUser
{
    internal sealed class ActivateUserUseCase : IActivateUserUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public ActivateUserUseCase(
            IUserRepository userRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork
            )
        {
            _userRepository = userRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> ExecuteAsync(ActivateUserDto request)
        {
            var user = await _userRepository.GetByIdAsync(request.Id);
            if (user is null)
                return Result.Failure(["El usuario no existe."], ErrorType.NotFound);

            if (!_currentUser.Role.CanManage(user.Role))
                return Result.Failure(["No tienes permisos sobre este usuario."], ErrorType.Forbidden);

            user.Activate();

            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
    }
}
