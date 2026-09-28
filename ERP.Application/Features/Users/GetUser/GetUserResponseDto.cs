using ERP.Domain.Users.Enums;

namespace ERP.Application.Features.Users.GetUser
{
    public sealed record GetUserResponseDto(
        Guid Id,
        string Name,
        string Email,
        UserRole Role,
        bool IsActive,
        DateTime CreatedAt,
        string? CreatedByName,
        DateTime? UpdatedAt,
        string? UpdatedByName
        )
    {
        public string RoleDescription => Role.Description;
    }
}
