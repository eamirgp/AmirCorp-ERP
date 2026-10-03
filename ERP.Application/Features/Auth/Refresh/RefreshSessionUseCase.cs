using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.Users;

namespace ERP.Application.Features.Auth.Refresh
{
    public interface IRefreshSessionUseCase
    {
        /// <param name="refreshToken">El token de la cookie del navegador, o null si no hay.</param>
        Task<Result<AuthSessionDto>> ExecuteAsync(string? refreshToken);
    }

    /// <summary>
    /// Renueva la sesión con el refresh token de la cookie: entrega un token de acceso nuevo y rota el refresh token.
    /// Si llega un token ya reemplazado (fuera del margen de las pestañas), alguien lo copió: se anula la sesión entera.
    /// </summary>
    internal sealed class RefreshSessionUseCase : IRefreshSessionUseCase
    {
        private const string Expired = "Tu sesión venció. Vuelve a iniciar sesión.";

        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUserRepository _userRepository;
        private readonly SessionTokens _sessionTokens;
        private readonly TimeProvider _timeProvider;
        private readonly IUnitOfWork _unitOfWork;

        public RefreshSessionUseCase(
            IRefreshTokenRepository refreshTokenRepository,
            IUserRepository userRepository,
            SessionTokens sessionTokens,
            TimeProvider timeProvider,
            IUnitOfWork unitOfWork
            )
        {
            _refreshTokenRepository = refreshTokenRepository;
            _userRepository = userRepository;
            _sessionTokens = sessionTokens;
            _timeProvider = timeProvider;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<AuthSessionDto>> ExecuteAsync(string? refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return Unauthorized(Expired);

            var now = _timeProvider.GetUtcNow().UtcDateTime;
            var current = await _refreshTokenRepository.GetByHashAsync(_sessionTokens.HashOf(refreshToken));
            if (current is null)
                return Unauthorized(Expired);

            // Primero el usuario: si lo desactivaron, se le dice eso (sus tokens ya quedaron anulados al desactivarlo).
            var user = await _userRepository.GetByIdAsync(current.UserId);
            if (user is null || !user.IsActive)
            {
                await _refreshTokenRepository.RevokeAllForUserAsync(current.UserId, now);
                await _unitOfWork.SaveChangesAsync();
                return Unauthorized(user is null ? Expired : User.DeactivatedError);
            }

            if (current.IsReuse(now))
            {
                await _refreshTokenRepository.RevokeFamilyAsync(current.FamilyId, now);
                await _unitOfWork.SaveChangesAsync();
                return Unauthorized(Expired);
            }

            // Otra pestaña acaba de renovar con este mismo token: la cookie del navegador ya es la nueva, así que solo
            // hace falta un token de acceso.
            if (current.RevokedAt is { } revokedAt)
                return Result<AuthSessionDto>.Success(_sessionTokens.AccessOnly(user, Min(revokedAt + RefreshToken.IdleLifetime, current.SessionExpiresAt)));

            if (current.RefreshError(now) is { } error)
                return Unauthorized(error);

            var (session, next) = _sessionTokens.Rotate(current, user, now);
            _refreshTokenRepository.Add(next);

            await _unitOfWork.SaveChangesAsync();

            return Result<AuthSessionDto>.Success(session);
        }

        private static Result<AuthSessionDto> Unauthorized(string message) =>
            Result<AuthSessionDto>.Failure([message], ErrorType.Unauthorized);

        private static DateTime Min(DateTime a, DateTime b) => a < b ? a : b;
    }
}
