using ERP.Application.Common.Results;
using ERP.Application.Contracts.Api;
using ERP.Application.Contracts.Persistence.Commands;

namespace ERP.Application.Features.Users.DeactivateUser
{
    internal sealed class DeactivateUserUseCase : IDeactivateUserUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ICurrentUser _currentUser;
        private readonly TimeProvider _timeProvider;
        private readonly IUnitOfWork _unitOfWork;

        public DeactivateUserUseCase(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            ICurrentUser currentUser,
            TimeProvider timeProvider,
            IUnitOfWork unitOfWork
            )
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _currentUser = currentUser;
            _timeProvider = timeProvider;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> ExecuteAsync(DeactivateUserDto request)
        {
            var user = await _userRepository.GetByIdAsync(request.Id);
            if (user is null)
                return Result.Failure(["El usuario no existe."], ErrorType.NotFound);

            // La misma regla del dominio, revisada antes para responder 403 con el mensaje.
            if (user.ManageError(_currentUser.Role) is { } permissionError)
                return Result.Failure([permissionError], ErrorType.Forbidden);

            user.Deactivate(_currentUser.Role);

            // Sus sesiones abiertas dejan de poder renovarse: al volver a activarlo tendrá que iniciar sesión.
            await _refreshTokenRepository.RevokeAllForUserAsync(user.Id, _timeProvider.GetUtcNow().UtcDateTime);

            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
    }
}
