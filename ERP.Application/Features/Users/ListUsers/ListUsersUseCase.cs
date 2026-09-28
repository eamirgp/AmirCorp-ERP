using ERP.Application.Contracts.Persistence.Queries;

namespace ERP.Application.Features.Users.ListUsers
{
    internal sealed class ListUsersUseCase : IListUsersUseCase
    {
        private readonly IUserQueries _userQueries;

        public ListUsersUseCase(IUserQueries userQueries) => _userQueries = userQueries;

        public async Task<IReadOnlyCollection<ListUsersResponseDto>> ExecuteAsync() =>
            await _userQueries.ListUsersAsync();
    }
}
