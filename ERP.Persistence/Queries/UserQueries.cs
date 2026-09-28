using ERP.Application.Contracts.Persistence.Queries;
using ERP.Application.Features.Accounts.GetMyProfile;
using ERP.Application.Features.Users.GetUser;
using ERP.Application.Features.Users.ListUsers;
using ERP.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Queries
{
    internal sealed class UserQueries : IUserQueries
    {
        private readonly ErpDbContext _context;

        public UserQueries(ErpDbContext context) => _context = context;

        public async Task<IReadOnlyCollection<ListUsersResponseDto>> ListUsersAsync() =>
            await _context.Users
            .AsNoTracking()
            .OrderBy(u => u.Name)
            .ThenBy(u => u.Id)
            .Select(u => new ListUsersResponseDto(
                u.Id,
                u.Name,
                u.Email,
                u.Role,
                u.IsActive
                ))
            .ToArrayAsync();

        public async Task<GetUserResponseDto?> GetUserAsync(GetUserDto getUserDto) =>
            await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == getUserDto.Id)
            .Select(u => new GetUserResponseDto(
                u.Id,
                u.Name,
                u.Email,
                u.Role,
                u.IsActive,
                u.CreatedAt,
                _context.Users.Where(c => c.Id == u.CreatedBy).Select(c => c.Name).FirstOrDefault(),
                u.UpdatedAt,
                _context.Users.Where(c => c.Id == u.UpdatedBy).Select(c => c.Name).FirstOrDefault()
                ))
            .FirstOrDefaultAsync();

        public async Task<GetMyProfileResponseDto?> GetMyProfileAsync(Guid id) =>
            await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new GetMyProfileResponseDto(
                u.Id,
                u.Name,
                u.Email,
                u.Role,
                u.IsActive
                ))
            .FirstOrDefaultAsync();
    }
}
