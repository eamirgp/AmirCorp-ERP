using ERP.Domain.Users.Enums;

namespace ERP.Application.Features.Auth.Session
{
    /// <summary>Lo que importa del usuario en cada pedido: si sigue activo y su rol de hoy.</summary>
    public sealed record SessionUserDto(bool IsActive, UserRole Role);
}
