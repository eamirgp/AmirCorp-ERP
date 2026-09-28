using ERP.Domain.Users.Enums;

namespace ERP.Application.Features.Accounts.GetMyProfile
{
    public sealed record GetMyProfileResponseDto(
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
