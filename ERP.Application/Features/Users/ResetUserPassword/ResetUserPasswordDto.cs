namespace ERP.Application.Features.Users.ResetUserPassword
{
    public sealed record ResetUserPasswordDto(
        Guid Id,
        string NewPassword
        );
}
