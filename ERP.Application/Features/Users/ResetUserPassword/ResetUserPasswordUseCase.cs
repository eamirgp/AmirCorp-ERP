using ERP.Application.Common.Results;
using ERP.Application.Contracts.Api;
using ERP.Application.Contracts.Infrastructure;
using ERP.Application.Contracts.Persistence.Commands;

namespace ERP.Application.Features.Users.ResetUserPassword
{
    internal sealed class ResetUserPasswordUseCase : IResetUserPasswordUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IPasswordService _passwordService;
        private readonly TimeProvider _timeProvider;
        private readonly IUnitOfWork _unitOfWork;

        public ResetUserPasswordUseCase(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            ICurrentUser currentUser,
            IPasswordService passwordService,
            TimeProvider timeProvider,
            IUnitOfWork unitOfWork
            )
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _currentUser = currentUser;
            _passwordService = passwordService;
            _timeProvider = timeProvider;
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

            // Quien conocía la contraseña anterior pierde las sesiones que tuviera abiertas: entra solo con la nueva.
            await _refreshTokenRepository.RevokeAllForUserAsync(user.Id, _timeProvider.GetUtcNow().UtcDateTime);

            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
    }
}
