using ERP.Application.Features.Users.ChangeUserRole;
using ERP.Domain.Users;
using ERP.Domain.Users.Enums;

namespace ERP.Api.Controllers.Users.Requests
{
    public sealed record ChangeUserRoleRequest(
        UserRole? Role,
        // Versión que se abrió en el formulario: si otra persona lo cambió mientras tanto, no se pisa su cambio.
        uint? RowVersion
        )
    {
        // La misma regla del dominio, revisada antes para responder con el mensaje.
        public IReadOnlyCollection<string> Validate() =>
            new[] { User.RoleError(Role), RowVersion is null ? "Falta la versión del usuario. Vuelve a abrir el formulario." : null }
                .OfType<string>()
                .ToArray();

        public ChangeUserRoleDto ToDto(Guid id) =>
            new(id, Role!.Value, RowVersion!.Value);
    }
}
