using ERP.Api.Common;
using ERP.Application.Features.Users.ListUsers;
using ERP.Domain.Users;
using ERP.Domain.Users.Enums;

namespace ERP.Api.Controllers.Users.Requests
{
    /// <summary>La lista de usuarios: búsqueda, estado y rol. Sin filtros trae todos.</summary>
    /// <param name="SearchTerm">Por nombre (sin mayúsculas ni tildes) o por parte del correo.</param>
    /// <param name="IsActive">true solo activos, false solo inactivos; sin valor, todos.</param>
    /// <param name="Role">Solo los de este rol; sin valor, todos.</param>
    public sealed record ListUsersRequest(string? SearchTerm, bool? IsActive, UserRole? Role)
    {
        // Un texto que no es un rol (?role=Jefe) ya lo rechaza ASP.NET al leerlo; esta es la misma regla del dominio por si
        // llega un valor que el enum acepta pero no es un rol.
        public IReadOnlyCollection<string> Validate() =>
            Role is null || User.RoleError(Role) is not { } error ? [] : [error];

        public ListUsersDto ToDto() => new(new ListFilterRequest(SearchTerm, IsActive).ToDto(), Role);
    }
}
