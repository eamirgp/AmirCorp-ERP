using ERP.Application.Common.Results;
using ERP.Application.Contracts.Api;
using ERP.Application.Contracts.Infrastructure;
using ERP.Application.Contracts.Persistence.Commands;

namespace ERP.Application.Features.Users.ResetUserPassword
{
    internal sealed class ResetUserPasswordUseCase : IResetUserPasswordUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IPasswordService _passwordService;
        private readonly IUnitOfWork _unitOfWork;

        public ResetUserPasswordUseCase(
            IUserRepository userRepository,
            ICurrentUser currentUser,
            IPasswordService passwordService,
            IUnitOfWork unitOfWork
            )
        {
            _userRepository = userRepository;
            _currentUser = currentUser;
            _passwordService = passwordService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> ExecuteAsync(ResetUserPasswordDto request)
        {
            var user = await _userRepository.GetByIdAsync(request.Id);
            if (user is null)
                return Result.Failure(["El usuario no existe."], ErrorType.NotFound);

            // La misma regla del dominio, revisada antes para responder 403 con el mensaje.
            if (user.ManageError(_currentUser.Role) is { } permissionError)
                return Result.Failure([permissionError], ErrorType.Forbidden);

            user.ResetPassword(request.NewPassword, _passwordService.Hash, _currentUser.Role);

            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
    }
}
