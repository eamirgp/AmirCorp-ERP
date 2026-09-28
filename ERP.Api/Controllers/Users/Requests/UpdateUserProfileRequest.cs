using ERP.Application.Features.Users.UpdateUserProfile;
using ERP.Domain.Users;
using System.Net.Mail;

namespace ERP.Api.Controllers.Users.Requests
{
    public sealed record UpdateUserProfileRequest(
        string? Name,
        string? Email
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

            return errors;
        }

        public UpdateUserProfileDto ToDto(Guid id) =>
            new(id, Name!, Email!);
    }
}
