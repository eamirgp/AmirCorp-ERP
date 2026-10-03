using ERP.Application.Features.Accounts.GetMyProfile;
using ERP.Application.Features.Auth.Session;
using ERP.Application.Features.Users.GetUser;
using ERP.Application.Features.Users.ListUsers;

namespace ERP.Application.Contracts.Persistence.Queries
{
    public interface IUserQueries
    {
        Task<IReadOnlyCollection<ListUsersResponseDto>> ListUsersAsync(ListUsersDto listUsersDto);
        Task<GetUserResponseDto?> GetUserAsync(GetUserDto getUserDto);
        Task<GetMyProfileResponseDto?> GetMyProfileAsync(Guid id);

        /// <summary>Si el usuario sigue activo y su rol actual, o null si ya no existe. Se consulta en cada pedido.</summary>
        Task<SessionUserDto?> GetSessionUserAsync(Guid id);
    }
}
