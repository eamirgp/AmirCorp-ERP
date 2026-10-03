using ERP.Api.Json;
using ERP.Application.Features.Users.ResetUserPassword;
using ERP.Domain.Users;
using System.Text.Json.Serialization;

namespace ERP.Api.Controllers.Users.Requests
{
    public sealed record ResetUserPasswordRequest(
        [property: JsonConverter(typeof(RawStringConverter))]
        string? NewPassword
        )
    {
        // La misma regla del dominio, revisada antes para responder con el mensaje.
        public IReadOnlyCollection<string> Validate() =>
            User.PasswordError(NewPassword) is { } error ? [error] : [];

        public ResetUserPasswordDto ToDto(Guid id) =>
            new(id, NewPassword!);
    }
}
