using ERP.Domain.Users;
using ERP.Domain.Users.Enums;

namespace ERP.Application.Contracts.Persistence.Commands
{
    public interface IUserRepository
    {
        void Add(User user);
        Task<User?> GetByIdAsync(Guid id);
        Task<bool> EmailExistsAsync(string email, Guid? excludeId = null);
        Task<User?> GetByEmailAsync(string email);
        Task<bool> RoleExistsAsync(UserRole role);
    }
}
