using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.Users;
using ERP.Domain.Users.Enums;
using ERP.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Commands
{
    internal sealed class UserRepository : IUserRepository
    {
        private readonly ErpDbContext _context;

        public UserRepository(ErpDbContext context) => _context = context;

        public void Add(User user) =>
            _context.Users
            .Add(user);

        public async Task<User?> GetByIdAsync(Guid id) =>
            await _context.Users
            .FindAsync(id);

        public async Task<bool> EmailExistsAsync(string email, Guid? excludeId = null)
        {
            var normalizedEmail = User.NormalizeEmail(email);

            return await _context.Users
                .Where(u => excludeId == null || u.Id != excludeId)
                .AnyAsync(u => u.Email == normalizedEmail);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            var normalizedEmail = User.NormalizeEmail(email);

            return await _context.Users
                .SingleOrDefaultAsync(u => u.Email == normalizedEmail);
        }

        public async Task<bool> RoleExistsAsync(UserRole role) =>
            await _context.Users
            .AnyAsync(u => u.Role == role);

        public uint VersionOf(User user) =>
            _context.Entry(user).Property<uint>("RowVersion").CurrentValue;
    }
}
