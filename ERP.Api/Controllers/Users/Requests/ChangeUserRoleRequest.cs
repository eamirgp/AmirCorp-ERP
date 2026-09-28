using ERP.Application.Features.Users.ChangeUserRole;
using ERP.Domain.Users.Enums;

namespace ERP.Api.Controllers.Users.Requests
{
    public sealed record ChangeUserRoleRequest(
        UserRole? Role
        )
    {
        public IReadOnlyCollection<string> Validate()
        {
            var errors = new List<string>();

            if (Role is null)
                errors.Add("El rol es requerido.");
            else if (!Enum.IsDefined(Role.Value))
                errors.Add("El rol es inválido.");

            return errors;
        }

        public ChangeUserRoleDto ToDto(Guid id) =>
            new(id, Role!.Value);
    }
}
