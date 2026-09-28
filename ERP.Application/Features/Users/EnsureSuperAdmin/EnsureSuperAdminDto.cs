namespace ERP.Application.Features.Users.EnsureSuperAdmin
{
    public sealed record EnsureSuperAdminDto(
        string? Name,
        string? Email,
        string? Password
        );
}
