using ERP.Application.Contracts.Api;
using ERP.Domain.Users.Enums;
using System.Security.Claims;

namespace ERP.Api.Services
{
    internal sealed class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool IsAuthenticated =>
            _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

        public Guid Id =>
            Guid.TryParse(FindClaim(ClaimTypes.NameIdentifier), out var id)
            ? id
            : throw new InvalidOperationException("El identificador del usuario no está presente en el token.");

        public UserRole Role =>
            Enum.TryParse<UserRole>(FindClaim(ClaimTypes.Role), out var role)
            ? role
            : throw new InvalidOperationException("El rol del usuario no está presente o es inválido en el token.");

        private string? FindClaim(string claimType) =>
            _httpContextAccessor.HttpContext?.User.FindFirstValue(claimType);
    }
}
