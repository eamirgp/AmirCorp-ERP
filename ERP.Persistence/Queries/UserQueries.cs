using ERP.Application.Contracts.Persistence.Queries;
using ERP.Application.Features.Accounts.GetMyProfile;
using ERP.Application.Features.Auth.Session;
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

        public async Task<IReadOnlyCollection<ListUsersResponseDto>> ListUsersAsync(ListUsersDto listUsersDto)
        {
            var filter = listUsersDto.Filter;
            var query = _context.Users.AsNoTracking();

            // Por el nombre (sin mayúsculas ni tildes) o por parte del correo.
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = SearchText.Normalize(filter.SearchTerm);
                query = query.Where(u => EF.Functions.Unaccent(u.Name.ToLower()).Contains(term) || u.Email.Contains(term));
            }

            if (filter.IsActive is { } isActive)
                query = query.Where(u => u.IsActive == isActive);

            if (listUsersDto.Role is { } role)
                query = query.Where(u => u.Role == role);

            return await query
            .OrderBy(u => u.Name)
            .ThenBy(u => u.Id)
            .Select(u => new ListUsersResponseDto(
                u.Id,
                u.Name,
                u.Email,
                u.Role,
                u.IsActive,
                EF.Property<uint>(u, "RowVersion")
                ))
            .ToArrayAsync();
        }

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

        public async Task<SessionUserDto?> GetSessionUserAsync(Guid id) =>
            await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new SessionUserDto(u.IsActive, u.Role))
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
