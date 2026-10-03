using ERP.Application.Contracts.Api;
using ERP.Application.Contracts.Persistence.Queries;
using ERP.Domain.Users.Enums;

namespace ERP.Application.Features.Users.ListUsers
{
    internal sealed class ListUsersUseCase : IListUsersUseCase
    {
        private readonly IUserQueries _userQueries;
        private readonly ICurrentUser _currentUser;

        public ListUsersUseCase(IUserQueries userQueries, ICurrentUser currentUser)
        {
            _userQueries = userQueries;
            _currentUser = currentUser;
        }

        public async Task<IReadOnlyCollection<ListUsersResponseDto>> ExecuteAsync(ListUsersDto listUsersDto) =>
            (await _userQueries.ListUsersAsync(listUsersDto))
            // La misma regla que User.ManageError: solo se gestiona a un usuario de rol menor (nunca a uno mismo).
            .Select(u => u with { CanManage = _currentUser.Role.CanManage(u.Role) })
            .ToArray();
    }
}
