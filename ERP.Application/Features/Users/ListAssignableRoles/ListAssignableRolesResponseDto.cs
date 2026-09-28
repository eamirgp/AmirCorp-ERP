using ERP.Domain.Users.Enums;

namespace ERP.Application.Features.Users.ListAssignableRoles
{
    public sealed record ListAssignableRolesResponseDto(
        UserRole UserRole,
        string Description
        );
}
