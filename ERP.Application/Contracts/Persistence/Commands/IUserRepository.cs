using ERP.Domain.Users;
using ERP.Domain.Users.Enums;

namespace ERP.Application.Contracts.Persistence.Commands
{
    public interface IUserRepository
    {
        void Add(User user);
        Task<User?> GetByIdAsync(Guid id);
        /// <summary>El usuario que ya tiene ese correo (sin contar el que se edita), o null.</summary>
        Task<User?> FindByEmailAsync(string email, Guid? excludeId = null);
        Task<User?> GetByEmailAsync(string email);
        Task<bool> RoleExistsAsync(UserRole role);

        /// <summary>Versión actual del usuario en la base (cambia con cada modificación).</summary>
        uint VersionOf(User user);
    }
}
