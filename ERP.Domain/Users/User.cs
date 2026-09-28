using ERP.Domain.Common;
using ERP.Domain.Users.Enums;
using System.Net.Mail;

namespace ERP.Domain.Users
{
    public sealed class User : AuditableEntity
    {
        public const int NameMaxLength = 100;
        public const int EmailMaxLength = 100;
        public const int PasswordMinLength = 8;

        public string Name { get; private set; }
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
        public UserRole Role { get; private set; }
        public bool IsActive { get; private set; }

        private User(Guid id, string name, string email, string passwordHash, UserRole role, bool isActive) : base(id)
        {
            Name = name;
            Email = email;
            PasswordHash = passwordHash;
            Role = role;
            IsActive = isActive;
        }

        public static User Create(string name, string email, string passwordHash, UserRole role) =>
            new(Guid.CreateVersion7(), ValidateName(name), ValidateEmail(email), ValidatePasswordHash(passwordHash), ValidateRole(role), isActive: true);

        public static string NormalizeEmail(string email) =>
            email.ToLowerInvariant();

        public void UpdateName(string name) =>
            Name = ValidateName(name);

        public void UpdateEmail(string email) =>
            Email = ValidateEmail(email);

        public void UpdatePassword(string passwordHash) =>
            PasswordHash = ValidatePasswordHash(passwordHash);

        public void UpdateRole(UserRole role) =>
            Role = ValidateRole(role);

        public void Activate() =>
            IsActive = true;

        public void Deactivate() =>
            IsActive = false;

        private static string ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("El nombre es requerido.");

            if (name.Length > NameMaxLength)
                throw new DomainException($"El nombre no puede exceder los {NameMaxLength} caracteres.");

            return name;
        }

        private static string ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new DomainException("El correo es requerido.");

            if (email.Length > EmailMaxLength)
                throw new DomainException($"El correo no puede exceder los {EmailMaxLength} caracteres.");

            if (!MailAddress.TryCreate(email, out _))
                throw new DomainException("El formato del correo es inválido.");

            return NormalizeEmail(email);
        }

        private static string ValidatePasswordHash(string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new DomainException("El hash de la contraseña es requerido.");

            return passwordHash;
        }

        private static UserRole ValidateRole(UserRole role)
        {
            if (!Enum.IsDefined(role))
                throw new DomainException("El rol es inválido.");

            return role;
        }
    }
}
