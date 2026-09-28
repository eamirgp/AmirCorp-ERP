using ERP.Application.Features.Accounts.GetMyProfile;
using ERP.Application.Features.Users.GetUser;
using ERP.Application.Features.Users.ListUsers;

namespace ERP.Application.Contracts.Persistence.Queries
{
    public interface IUserQueries
    {
        Task<IReadOnlyCollection<ListUsersResponseDto>> ListUsersAsync();
        Task<GetUserResponseDto?> GetUserAsync(GetUserDto getUserDto);
        Task<GetMyProfileResponseDto?> GetMyProfileAsync(Guid id);
    }
}
