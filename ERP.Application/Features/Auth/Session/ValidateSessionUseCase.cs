using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Application.Contracts.Persistence.Queries;
using ERP.Domain.Users;
using ERP.Domain.Users.Enums;

namespace ERP.Application.Features.Auth.Session
{
    /// <summary>Lo que importa del usuario en cada pedido: si sigue activo y su rol de hoy.</summary>
    public sealed record SessionUserDto(bool IsActive, UserRole Role);

    public interface IValidateSessionUseCase
    {
        /// <param name="sessionId">La sesión del token (<see cref="RefreshToken.FamilyId"/>).</param>
        /// <returns>El rol actual del usuario, o 401 si ya no existe, está desactivado o su sesión se cerró.</returns>
        Task<Result<UserRole>> ExecuteAsync(Guid userId, Guid sessionId);
    }

    /// <summary>
    /// Revisa en cada pedido que el usuario del token siga activo y toma su rol actual, no el que tenía al iniciar
    /// sesión. Así desactivar a alguien lo saca del sistema al momento y bajarle el rol le quita los permisos de
    /// inmediato, como en Odoo o SAP, que revisan la sesión en el servidor. También revisa que la sesión siga abierta:
    /// después de "Cerrar sesión" o de restablecer la contraseña, el token de acceso deja de servir aunque no haya vencido.
    /// </summary>
    internal sealed class ValidateSessionUseCase : IValidateSessionUseCase
    {
        public const string SessionClosed = "Tu sesión se cerró. Vuelve a iniciar sesión.";

        private readonly IUserQueries _userQueries;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly TimeProvider _timeProvider;

        public ValidateSessionUseCase(IUserQueries userQueries, IRefreshTokenRepository refreshTokenRepository, TimeProvider timeProvider)
        {
            _userQueries = userQueries;
            _refreshTokenRepository = refreshTokenRepository;
            _timeProvider = timeProvider;
        }

        public async Task<Result<UserRole>> ExecuteAsync(Guid userId, Guid sessionId)
        {
            var user = await _userQueries.GetSessionUserAsync(userId);

            if (user is null)
                return Result<UserRole>.Failure(["Tu usuario ya no existe. Habla con el administrador."], ErrorType.Unauthorized);

            if (!user.IsActive)
                return Result<UserRole>.Failure([User.DeactivatedError], ErrorType.Unauthorized);

            var now = _timeProvider.GetUtcNow().UtcDateTime;
            var open = (await _refreshTokenRepository.ListUnrevokedInFamilyAsync(sessionId))
                .Any(t => t.UserId == userId && t.IsActive(now));
            if (!open)
                return Result<UserRole>.Failure([SessionClosed], ErrorType.Unauthorized);

            return Result<UserRole>.Success(user.Role);
        }
    }
}
