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
        public IReadOnlyCollection<string> Validate()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(NewPassword))
                errors.Add("La contraseña es requerida.");
            else if (NewPassword.Length < User.PasswordMinLength)
                errors.Add($"La contraseña debe tener al menos {User.PasswordMinLength} caracteres.");

            return errors;
        }

        public ResetUserPasswordDto ToDto(Guid id) =>
            new(id, NewPassword!);
    }
}
