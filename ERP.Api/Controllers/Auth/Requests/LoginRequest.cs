using ERP.Api.Json;
using ERP.Application.Features.Auth.Login;
using ERP.Domain.Users;
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

            // Solo el largo (no el formato): así un correo enorme no llega a la cuenta de intentos fallidos, y no se le
            // revela nada a quien prueba.
            if (string.IsNullOrWhiteSpace(Email))
                errors.Add("El correo es requerido.");
            else if (Email.Length > User.EmailMaxLength)
                errors.Add($"El correo no puede exceder los {User.EmailMaxLength} caracteres.");

            if (string.IsNullOrWhiteSpace(Password))
                errors.Add("La contraseña es requerida.");

            return errors;
        }

        public LoginDto ToDto(string? clientIp) =>
            new(Email!, Password!, clientIp);
    }
}
