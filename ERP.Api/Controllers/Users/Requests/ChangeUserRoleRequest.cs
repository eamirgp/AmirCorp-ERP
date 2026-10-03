using ERP.Application.Features.Users.ChangeUserRole;
using ERP.Domain.Users;
using ERP.Domain.Users.Enums;

namespace ERP.Api.Controllers.Users.Requests
{
    public sealed record ChangeUserRoleRequest(
        UserRole? Role
        )
    {
        // La misma regla del dominio, revisada antes para responder con el mensaje.
        public IReadOnlyCollection<string> Validate() =>
            User.RoleError(Role) is { } error ? [error] : [];

        public ChangeUserRoleDto ToDto(Guid id) =>
            new(id, Role!.Value);
    }
}
