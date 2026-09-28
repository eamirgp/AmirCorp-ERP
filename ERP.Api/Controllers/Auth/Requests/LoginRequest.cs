using ERP.Api.Json;
using ERP.Application.Features.Auth.Login;
using System.Text.Json.Serialization;

namespace ERP.Api.Controllers.Auth.Requests
{
    public sealed record LoginRequest(
        string? Email,
        [property: JsonConverter(typeof(RawStringConverter))]
        string? Password
        )
    {
        public IReadOnlyCollection<string> Validate()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Email))
                errors.Add("El correo es requerido.");

            if (string.IsNullOrWhiteSpace(Password))
                errors.Add("La contraseña es requerida.");

            return errors;
        }

        public LoginDto ToDto() =>
            new(Email!, Password!);
    }
}
