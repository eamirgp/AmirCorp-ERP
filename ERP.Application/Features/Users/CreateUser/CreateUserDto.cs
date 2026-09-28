using ERP.Domain.Users.Enums;

namespace ERP.Application.Features.Users.CreateUser
{
    public sealed record CreateUserDto(
        string Name,
        string Email,
        string Password,
        UserRole Role
        );
}
