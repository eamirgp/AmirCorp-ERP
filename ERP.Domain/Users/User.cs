using System.Net.Mail;
using System.Text;
using ERP.Domain.Common;
using ERP.Domain.Users.Enums;

namespace ERP.Domain.Users
{
    /// <summary>
    /// Usuario del sistema. Las reglas se exponen como funciones <c>…Error</c> que devuelven el mensaje o null: el
    /// dominio lanza el error con ellas y la API y los casos de uso las llaman antes para responder con el mensaje.
    /// Quién puede hacer qué también lo decide aquí: cada cambio recibe el rol de quien lo hace (<c>actorRole</c>), y
    /// solo se gestiona a un usuario de rol menor (un administrador no modifica a otro administrador ni a sí mismo).
    /// </summary>
    public sealed class User : AuditableEntity
    {
        public const int NameMaxLength = 100;
        public const int EmailMaxLength = 100;
        public const int PasswordMinLength = 8;
        // BCrypt solo usa los primeros 72 bytes: más larga, el resto se ignoraría sin avisar.
        public const int PasswordMaxBytes = 72;

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

        /// <summary>Un usuario creado por otro, que solo puede darle un rol menor al suyo.</summary>
        /// <param name="hashPassword">Convierte la contraseña en su hash (lo hace Infrastructure); el dominio nunca guarda la contraseña.</param>
        public static User Create(string name, string email, string password, Func<string, string> hashPassword, UserRole role, UserRole actorRole)
        {
            Throw(AssignRoleError(actorRole, role));
            return New(name, email, password, hashPassword, role);
        }

        /// <summary>El primer SuperAdmin, que crea el sistema al arrancar si no existe ninguno (decisión 7).</summary>
        public static User CreateSuperAdmin(string name, string email, string password, Func<string, string> hashPassword) =>
            New(name, email, password, hashPassword, UserRole.SuperAdmin);

        /// <summary>El correo tal como se guarda y se compara: sin espacios alrededor y en minúsculas.</summary>
        public static string NormalizeEmail(string email) =>
            email.Trim().ToLowerInvariant();

        public void UpdateProfile(string name, string email, UserRole actorRole)
        {
            Throw(ManageError(actorRole));
            Throw(NameError(name));
            Throw(EmailError(email));

            Name = TextNormalizer.CollapseSpaces(name);
            Email = NormalizeEmail(email);
        }

        public void ResetPassword(string password, Func<string, string> hashPassword, UserRole actorRole)
        {
            Throw(ManageError(actorRole));
            Throw(PasswordError(password));

            PasswordHash = hashPassword(password);
        }

        public void ChangeRole(UserRole role, UserRole actorRole)
        {
            Throw(ManageError(actorRole));
            Throw(RoleError(role));
            Throw(AssignRoleError(actorRole, role));

            Role = role;
        }

        /// <summary>Si ya estaba activo, no cambia nada.</summary>
        public void Activate(UserRole actorRole)
        {
            Throw(ManageError(actorRole));
            IsActive = true;
        }

        /// <summary>Si ya estaba inactivo, no cambia nada.</summary>
        public void Deactivate(UserRole actorRole)
        {
            Throw(ManageError(actorRole));
            IsActive = false;
        }

        /// <summary>Qué impide a quien tiene ese rol modificar a este usuario, o null si puede.</summary>
        public string? ManageError(UserRole actorRole) =>
            actorRole.CanManage(Role) ? null : "No tienes permisos sobre este usuario.";

        /// <summary>Qué impide a quien tiene ese rol dar el rol indicado, o null si puede (solo roles menores al suyo).</summary>
        public static string? AssignRoleError(UserRole actorRole, UserRole role) =>
            actorRole.CanManage(role) ? null : "No tienes permisos para asignar este rol.";

        /// <summary>Qué tiene de malo el nombre, o null si está bien.</summary>
        public static string? NameError(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "El nombre es requerido.";

            if (TextNormalizer.CollapseSpaces(name).Length > NameMaxLength)
                return $"El nombre no puede exceder los {NameMaxLength} caracteres.";

            return null;
        }

        /// <summary>Qué tiene de malo el correo, o null si está bien.</summary>
        public static string? EmailError(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return "El correo es requerido.";

            var normalized = NormalizeEmail(email);
            if (normalized.Length > EmailMaxLength)
                return $"El correo no puede exceder los {EmailMaxLength} caracteres.";

            if (!MailAddress.TryCreate(normalized, out _))
                return "El formato del correo es inválido.";

            return null;
        }

        /// <summary>Qué tiene de malo la contraseña, o null si está bien. Se revisa tal como se escribió, con sus espacios.</summary>
        public static string? PasswordError(string? password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return "La contraseña es requerida.";

            if (password.Length < PasswordMinLength)
                return $"La contraseña debe tener al menos {PasswordMinLength} caracteres.";

            if (Encoding.UTF8.GetByteCount(password) > PasswordMaxBytes)
                return $"La contraseña es demasiado larga: usa como máximo {PasswordMaxBytes} letras y números (menos si lleva tildes o símbolos).";

            return null;
        }

        /// <summary>Qué tiene de malo el rol, o null si está bien.</summary>
        public static string? RoleError(UserRole? role) =>
            role switch
            {
                null => "El rol es requerido.",
                { } value when !Enum.IsDefined(value) => "El rol es inválido.",
                _ => null
            };

        private static User New(string name, string email, string password, Func<string, string> hashPassword, UserRole role)
        {
            Throw(NameError(name));
            Throw(EmailError(email));
            Throw(PasswordError(password));
            Throw(RoleError(role));

            return new(Guid.CreateVersion7(), TextNormalizer.CollapseSpaces(name), NormalizeEmail(email), hashPassword(password), role, isActive: true);
        }

        private static void Throw(string? error)
        {
            if (error is not null)
                throw new DomainException(error);
        }
    }
}
