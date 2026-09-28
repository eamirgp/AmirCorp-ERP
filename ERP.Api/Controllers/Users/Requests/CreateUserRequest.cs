using ERP.Api.Json;
using ERP.Application.Features.Users.CreateUser;
using ERP.Domain.Users;
using ERP.Domain.Users.Enums;
using System.Net.Mail;
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
        public IReadOnlyCollection<string> Validate()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Name))
                errors.Add("El nombre es requerido.");
            else if (Name.Length > User.NameMaxLength)
                errors.Add($"El nombre no puede exceder los {User.NameMaxLength} caracteres.");

            if (string.IsNullOrWhiteSpace(Email))
                errors.Add("El correo es requerido.");
            else if (Email.Length > User.EmailMaxLength)
                errors.Add($"El correo no puede exceder los {User.EmailMaxLength} caracteres.");
            else if (!MailAddress.TryCreate(Email, out _))
                errors.Add("El formato del correo es inválido.");

            if (string.IsNullOrWhiteSpace(Password))
                errors.Add("La contraseña es requerida.");
            else if (Password.Length < User.PasswordMinLength)
                errors.Add($"La contraseña debe tener al menos {User.PasswordMinLength} caracteres.");

            if (Role is null)
                errors.Add("El rol es requerido.");
            else if (!Enum.IsDefined(Role.Value))
                errors.Add("El rol es inválido.");

            return errors;
        }

        public CreateUserDto ToDto() =>
            new(Name!, Email!, Password!, Role!.Value);
    }
}
