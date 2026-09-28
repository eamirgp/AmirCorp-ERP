using ERP.Domain.Users.Enums;

namespace ERP.Application.Features.Users.ListAssignableRoles
{
    internal sealed class ListAssignableRolesUseCase : IListAssignableRolesUseCase
    {
        private static readonly IReadOnlyCollection<ListAssignableRolesResponseDto> _userRoles =
            Enum.GetValues<UserRole>()
            .Where(ur => ur != UserRole.SuperAdmin)
            .Select(ur => new ListAssignableRolesResponseDto(ur, ur.Description))
            .ToArray();

        public Task<IReadOnlyCollection<ListAssignableRolesResponseDto>> ExecuteAsync() =>
            Task.FromResult(_userRoles);
    }
}
