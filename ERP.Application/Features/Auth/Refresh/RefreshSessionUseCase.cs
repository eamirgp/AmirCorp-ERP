using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.Users;

namespace ERP.Application.Features.Auth.Refresh
{
    /// <summary>
    /// Renueva la sesión con el refresh token de la cookie: entrega un token de acceso nuevo y rota el refresh token.
    /// Si llega un token ya reemplazado (fuera del margen de las pestañas), alguien lo copió: se anula la sesión entera.
    /// </summary>
    internal sealed class RefreshSessionUseCase : IRefreshSessionUseCase
    {
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

        /// <param name="refreshToken">El token de la cookie del navegador, o null si no hay.</param>
        public async Task<Result<AuthSessionDto>> ExecuteAsync(string? refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return Unauthorized(RefreshToken.ExpiredError);

            var now = _timeProvider.GetUtcNow().UtcDateTime;
            var current = await _refreshTokenRepository.GetByHashAsync(_sessionTokens.HashOf(refreshToken));
            if (current is null)
                return Unauthorized(RefreshToken.ExpiredError);

            // Primero el usuario: si lo desactivaron, se le dice eso (sus tokens ya quedaron anulados al desactivarlo).
            var user = await _userRepository.GetByIdAsync(current.UserId);
            if (user is null || !user.IsActive)
            {
                await _refreshTokenRepository.RevokeAllForUserAsync(current.UserId, now);
                await _unitOfWork.SaveChangesAsync();
                return Unauthorized(user is null ? RefreshToken.ExpiredError : User.DeactivatedError);
            }

            if (current.IsReuse(now))
            {
                await _refreshTokenRepository.RevokeFamilyAsync(current.FamilyId, now);
                await _unitOfWork.SaveChangesAsync();
                return Unauthorized(RefreshToken.ExpiredError);
            }

            if (current.IsRevoked)
                return await ContinueReplacedAsync(current, user, now);

            if (current.RefreshError(now) is { } error)
                return Unauthorized(error);

            return await RotateAsync(current, user, now);
        }

        private async Task<Result<AuthSessionDto>> RotateAsync(RefreshToken current, User user, DateTime now)
        {
            var (session, next) = _sessionTokens.Rotate(current, user, now);
            _refreshTokenRepository.Add(next);

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (ConcurrencyException)
            {
                // Otro pedido renovó con este mismo token por milisegundos (la versión del token cambió): ese ganó y este
                // no guarda nada. No es un error para el usuario: recibe un token de acceso y la cookie queda la del otro.
                return await AccessOnlyIfOpenAsync(current.FamilyId, user, now);
            }

            return Result<AuthSessionDto>.Success(session);
        }

        /// <summary>Solo un token de acceso, sin tocar la cookie, si la sesión sigue abierta.</summary>
        private async Task<Result<AuthSessionDto>> AccessOnlyIfOpenAsync(Guid familyId, User user, DateTime now)
        {
            var open = (await _refreshTokenRepository.ListUnrevokedInFamilyAsync(familyId)).FirstOrDefault(t => t.IsActive(now));
            return open is null
                ? Unauthorized(RefreshToken.ExpiredError)
                : Result<AuthSessionDto>.Success(_sessionTokens.AccessOnly(user, familyId, open.ExpiresAt));
        }

        /// <summary>
        /// Llegó un token que se reemplazó hace segundos (dentro del margen). Puede ser otra pestaña que renovó al mismo
        /// tiempo, o el mismo navegador reintentando porque la respuesta con el token nuevo no le llegó (se cortó la
        /// conexión). Solo se sigue si la sesión sigue abierta: si en esos segundos se cerró sesión o se restableció la
        /// contraseña, ya no.
        /// </summary>
        private async Task<Result<AuthSessionDto>> ContinueReplacedAsync(RefreshToken current, User user, DateTime now)
        {
            // El siguiente nunca se usó: puede que no haya llegado al navegador. Se renueva desde él y la cookie queda al
            // día; si sí había llegado, el navegador simplemente la reemplaza por esta.
            var successor = await _refreshTokenRepository.GetByIdAsync(current.ReplacedById!.Value);
            if (successor is not null && successor.IsActive(now))
                return await RotateAsync(successor, user, now);

            // El siguiente ya se usó en este navegador: basta un token de acceso, sin tocar la cookie.
            return await AccessOnlyIfOpenAsync(current.FamilyId, user, now);
        }

        private static Result<AuthSessionDto> Unauthorized(string message) =>
            Result<AuthSessionDto>.Failure([message], ErrorType.Unauthorized);
    }
}
