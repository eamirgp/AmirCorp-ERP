using ERP.Application.Common.Results;
using ERP.Domain.Users;
using ERP.Domain.Users.Enums;

namespace ERP.Application.Features.Auth.Session
{
    public interface IValidateSessionUseCase
    {
        /// <param name="sessionId">La sesión del token (<see cref="RefreshToken.FamilyId"/>).</param>
        /// <returns>El rol actual del usuario, o 401 si ya no existe, está desactivado o su sesión se cerró.</returns>
        Task<Result<UserRole>> ExecuteAsync(Guid userId, Guid sessionId);
    }
}
