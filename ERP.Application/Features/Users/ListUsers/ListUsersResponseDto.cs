using ERP.Domain.Users.Enums;

namespace ERP.Application.Features.Users.ListUsers
{
    public sealed record ListUsersResponseDto(
        Guid Id,
        string Name,
        string Email,
        UserRole Role,
        bool IsActive,
        // Versión del usuario: el formulario la devuelve al editar para no pisar cambios de otra persona.
        uint RowVersion
        )
    {
        public string RoleDescription => Role.Description;

        /// <summary>"Activo" o "Inactivo", para la columna Estado.</summary>
        public string StatusDescription => IsActive ? "Activo" : "Inactivo";
    }
}
