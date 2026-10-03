using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Queries;
using ERP.Domain.Users;
using ERP.Domain.Users.Enums;

namespace ERP.Application.Features.Auth.Session
{
    /// <summary>Lo que importa del usuario en cada pedido: si sigue activo y su rol de hoy.</summary>
    public sealed record SessionUserDto(bool IsActive, UserRole Role);

    public interface IValidateSessionUseCase
    {
        /// <returns>El rol actual del usuario, o 401 si ya no existe o está desactivado.</returns>
        Task<Result<UserRole>> ExecuteAsync(Guid userId);
    }

    /// <summary>
    /// Revisa en cada pedido que el usuario del token siga activo y toma su rol actual, no el que tenía al iniciar
    /// sesión. Así desactivar a alguien lo saca del sistema al momento y bajarle el rol le quita los permisos de
    /// inmediato, como en Odoo o SAP, que revisan la sesión en el servidor.
    /// </summary>
    internal sealed class ValidateSessionUseCase : IValidateSessionUseCase
    {
        private readonly IUserQueries _userQueries;

        public ValidateSessionUseCase(IUserQueries userQueries) => _userQueries = userQueries;

        public async Task<Result<UserRole>> ExecuteAsync(Guid userId)
        {
            var user = await _userQueries.GetSessionUserAsync(userId);

            if (user is null)
                return Result<UserRole>.Failure(["Tu usuario ya no existe. Habla con el administrador."], ErrorType.Unauthorized);

            if (!user.IsActive)
                return Result<UserRole>.Failure([User.DeactivatedError], ErrorType.Unauthorized);

            return Result<UserRole>.Success(user.Role);
        }
    }
}
