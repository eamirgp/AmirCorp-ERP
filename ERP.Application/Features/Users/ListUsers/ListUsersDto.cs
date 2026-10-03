using ERP.Application.Common.Pagination;
using ERP.Domain.Users.Enums;

namespace ERP.Application.Features.Users.ListUsers
{
    /// <summary>La búsqueda y el estado de toda lista corta, y el rol (null = todos).</summary>
    public sealed record ListUsersDto(ListFilterDto Filter, UserRole? Role);
}
