using ERP.Application.Common.Results;
using ERP.Application.Contracts.Api;
using ERP.Application.Contracts.Persistence.Queries;

namespace ERP.Application.Features.Accounts.GetMyProfile
{
    internal sealed class GetMyProfileUseCase : IGetMyProfileUseCase
    {
        private readonly IUserQueries _userQueries;
        private readonly ICurrentUser _currentUser;

        public GetMyProfileUseCase(
            IUserQueries userQueries,
            ICurrentUser currentUser
            )
        {
            _userQueries = userQueries;
            _currentUser = currentUser;
        }

        public async Task<Result<GetMyProfileResponseDto>> ExecuteAsync() =>
            Result<GetMyProfileResponseDto>.FoundOr(await _userQueries.GetMyProfileAsync(_currentUser.Id), "Tu usuario ya no existe.");
    }
}
