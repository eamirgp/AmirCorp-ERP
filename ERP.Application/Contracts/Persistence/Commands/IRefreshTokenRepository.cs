using ERP.Domain.Users;

namespace ERP.Application.Contracts.Persistence.Commands
{
    public interface IRefreshTokenRepository
    {
        void Add(RefreshToken refreshToken);

        Task<RefreshToken?> GetByHashAsync(string tokenHash);

        /// <summary>Anula todos los tokens de una sesión (se detectó que alguien copió uno).</summary>
        Task RevokeFamilyAsync(Guid familyId, DateTime now);

        /// <summary>Anula todas las sesiones del usuario (desactivado, contraseña restablecida).</summary>
        Task RevokeAllForUserAsync(Guid userId, DateTime now);

        /// <summary>Borra los tokens del usuario que vencieron hace más de un día: ya no sirven ni para detectar robos.</summary>
        Task RemoveExpiredForUserAsync(Guid userId, DateTime now);
    }
}
