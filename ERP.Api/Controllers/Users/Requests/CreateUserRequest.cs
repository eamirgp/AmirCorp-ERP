using ERP.Api.Json;
using ERP.Application.Features.Users.CreateUser;
using ERP.Domain.Users;
using ERP.Domain.Users.Enums;
using System.Text.Json.Serialization;

namespace ERP.Api.Controllers.Users.Requests
{
    public sealed record CreateUserRequest(
        string? Name,
        string? Email,
        [property: JsonConverter(typeof(RawStringConverter))]
        string? Password,
        UserRole? Role
        )
    {
        // Las mismas reglas del dominio, revisadas antes para avisar todos los errores juntos.
        public IReadOnlyCollection<string> Validate() =>
            new[] { User.NameError(Name), User.EmailError(Email), User.PasswordError(Password), User.RoleError(Role) }
                .OfType<string>()
                .ToArray();

        public CreateUserDto ToDto() =>
            new(Name!, Email!, Password!, Role!.Value);
    }
}
