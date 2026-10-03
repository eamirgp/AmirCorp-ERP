using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Queries;

namespace ERP.Application.Features.Users.GetUser
{
    internal sealed class GetUserUseCase : IGetUserUseCase
    {
        private readonly IUserQueries _userQueries;

        public GetUserUseCase(IUserQueries userQueries) => _userQueries = userQueries;

        public async Task<Result<GetUserResponseDto>> ExecuteAsync(GetUserDto request) =>
            Result<GetUserResponseDto>.FoundOr(await _userQueries.GetUserAsync(request), "El usuario no existe.");
    }
}
