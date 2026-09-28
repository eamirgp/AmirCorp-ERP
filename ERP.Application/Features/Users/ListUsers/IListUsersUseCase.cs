using ERP.Application.Common.Interfaces;

namespace ERP.Application.Features.Users.ListUsers
{
    public interface IListUsersUseCase : IQueryUseCase<IReadOnlyCollection<ListUsersResponseDto>> { }
}
