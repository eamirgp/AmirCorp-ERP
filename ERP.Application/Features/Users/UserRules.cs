using ERP.Application.Common.Exceptions;

namespace ERP.Application.Features.Users
{
    /// <summary>Textos de los casos de uso de usuarios que no son reglas del usuario (esas están en <see cref="Domain.Users.User"/>).</summary>
    internal static class UserRules
    {
        public static readonly string ModifiedByOther = ConcurrencyException.EditedWhileOpenMessage("este usuario");
    }
}
