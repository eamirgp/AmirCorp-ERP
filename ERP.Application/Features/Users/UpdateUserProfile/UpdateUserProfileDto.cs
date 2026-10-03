namespace ERP.Application.Features.Users.UpdateUserProfile
{
    public sealed record UpdateUserProfileDto(
        Guid Id,
        string Name,
        string Email,
        // Versión que se abrió en el formulario.
        uint RowVersion
        );
}
