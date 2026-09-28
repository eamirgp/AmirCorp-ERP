using ERP.Application.Common.Interfaces;

namespace ERP.Application.Features.Users.ListAssignableRoles
{
    public interface IListAssignableRolesUseCase : IQueryUseCase<IReadOnlyCollection<ListAssignableRolesResponseDto>> { }
}
