using ERP.Domain.Users.Enums;

namespace ERP.Application.Features.Users.ChangeUserRole
{
    public sealed record ChangeUserRoleDto(
        Guid Id,
        UserRole Role,
        // Versión que se abrió en el formulario.
        uint RowVersion
        );
}
