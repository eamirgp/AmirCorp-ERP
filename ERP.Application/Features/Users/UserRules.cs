namespace ERP.Application.Features.Users
{
    /// <summary>Textos de los casos de uso de usuarios que no son reglas del usuario (esas están en <see cref="Domain.Users.User"/>).</summary>
    internal static class UserRules
    {
        public const string ModifiedByOther =
            "Otra persona modificó este usuario mientras lo editabas. Cierra el formulario y vuelve a abrirlo para ver los datos actuales.";
    }
}
