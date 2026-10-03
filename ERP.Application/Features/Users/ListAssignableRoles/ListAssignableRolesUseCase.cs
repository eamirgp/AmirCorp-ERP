using ERP.Application.Contracts.Api;
using ERP.Domain.Users;
using ERP.Domain.Users.Enums;

namespace ERP.Application.Features.Users.ListAssignableRoles
{
    /// <summary>
    /// Todos los roles, y cuáles puede dar quien consulta: solo los menores al suyo (<see cref="User.AssignRoleError"/>).
    /// Así el formulario no ofrece uno que después se rechazaría, y el filtro de la lista de usuarios los tiene todos.
    /// </summary>
    internal sealed class ListAssignableRolesUseCase : IListAssignableRolesUseCase
    {
        private readonly ICurrentUser _currentUser;

        public ListAssignableRolesUseCase(ICurrentUser currentUser) => _currentUser = currentUser;

        public Task<IReadOnlyCollection<ListAssignableRolesResponseDto>> ExecuteAsync() =>
            Task.FromResult<IReadOnlyCollection<ListAssignableRolesResponseDto>>(
                Enum.GetValues<UserRole>()
                    .Select(role => new ListAssignableRolesResponseDto(role, role.Description, User.AssignRoleError(_currentUser.Role, role) is null))
                    .ToArray());
    }
}
