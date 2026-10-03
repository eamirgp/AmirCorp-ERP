using ERP.Domain.Users.Enums;

namespace ERP.Application.Features.Users.ListAssignableRoles
{
    public sealed record ListAssignableRolesResponseDto(
        UserRole UserRole,
        string Description,
        // Si quien consulta puede dar este rol: el formulario solo ofrece estos; el filtro de la lista, todos.
        bool CanAssign
        );
}
