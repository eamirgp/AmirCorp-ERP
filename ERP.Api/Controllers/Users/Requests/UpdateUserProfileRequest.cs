using ERP.Application.Features.Users.UpdateUserProfile;
using ERP.Domain.Users;

namespace ERP.Api.Controllers.Users.Requests
{
    public sealed record UpdateUserProfileRequest(
        string? Name,
        string? Email
        )
    {
        // Las mismas reglas del dominio, revisadas antes para avisar todos los errores juntos.
        public IReadOnlyCollection<string> Validate() =>
            new[] { User.NameError(Name), User.EmailError(Email) }
                .OfType<string>()
                .ToArray();

        public UpdateUserProfileDto ToDto(Guid id) =>
            new(id, Name!, Email!);
    }
}
