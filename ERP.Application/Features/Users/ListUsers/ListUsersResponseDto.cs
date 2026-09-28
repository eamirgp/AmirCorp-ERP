using ERP.Domain.Users.Enums;

namespace ERP.Application.Features.Users.ListUsers
{
    public sealed record ListUsersResponseDto(
        Guid Id,
        string Name,
        string Email,
        UserRole Role,
        bool IsActive
        )
    {
        public string RoleDescription => Role.Description;
    }
}
