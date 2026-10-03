using ERP.Application.Features.Users.UpdateUserProfile;
using ERP.Domain.Users;

namespace ERP.Api.Controllers.Users.Requests
{
    public sealed record UpdateUserProfileRequest(
        string? Name,
        string? Email,
        // Versión que se abrió en el formulario: si otra persona lo cambió mientras tanto, no se pisa su cambio.
        uint? RowVersion
        )
    {
        // Las mismas reglas del dominio, revisadas antes para avisar todos los errores juntos.
        public IReadOnlyCollection<string> Validate() =>
            new[] { User.NameError(Name), User.EmailError(Email), RowVersion is null ? "Falta la versión del usuario. Vuelve a abrir el formulario." : null }
                .OfType<string>()
                .ToArray();

        public UpdateUserProfileDto ToDto(Guid id) =>
            new(id, Name!, Email!, RowVersion!.Value);
    }
}
