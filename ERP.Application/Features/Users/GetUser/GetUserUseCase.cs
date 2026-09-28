using ERP.Application.Contracts.Persistence.Queries;

namespace ERP.Application.Features.Users.GetUser
{
    internal sealed class GetUserUseCase : IGetUserUseCase
    {
        private readonly IUserQueries _userQueries;

        public GetUserUseCase(IUserQueries userQueries) => _userQueries = userQueries;

        public async Task<GetUserResponseDto?> ExecuteAsync(GetUserDto request) =>
            await _userQueries.GetUserAsync(request);
    }
}
