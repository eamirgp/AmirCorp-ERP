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

        /// <summary>Un usuario desactivado no entra ni sigue usando el sistema: al iniciar sesión y en cada pedido.</summary>
        public const string DeactivatedError = "Tu cuenta está desactivada. Si crees que es un error, habla con el administrador.";

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
            DomainException.ThrowIf(AssignRoleError(actorRole, role));
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
            DomainException.ThrowIf(ManageError(actorRole));
            DomainException.ThrowIf(NameError(name));
            DomainException.ThrowIf(EmailError(email));

            Name = NormalizeName(name);
            Email = NormalizeEmail(email);
        }

        /// <summary>
        /// Cambia la contraseña y cierra todas sus sesiones abiertas: quien tenía la anterior (por ejemplo, alguien que la
        /// vio) ya no puede seguir dentro.
        /// </summary>
        /// <param name="openSessions">Los refresh tokens del usuario que todavía no se anularon.</param>
        public void ResetPassword(string password, Func<string, string> hashPassword, UserRole actorRole, IReadOnlyCollection<RefreshToken> openSessions, DateTime now)
        {
            DomainException.ThrowIf(ManageError(actorRole));
            DomainException.ThrowIf(PasswordError(password));

            PasswordHash = hashPassword(password);
            CloseSessions(openSessions, now);
        }

        public void ChangeRole(UserRole role, UserRole actorRole)
        {
            DomainException.ThrowIf(ManageError(actorRole));
            DomainException.ThrowIf(RoleError(role));
            DomainException.ThrowIf(AssignRoleError(actorRole, role));

            Role = role;
        }

        /// <summary>Si ya estaba activo, no cambia nada.</summary>
        public void Activate(UserRole actorRole)
        {
            DomainException.ThrowIf(ManageError(actorRole));
            IsActive = true;
        }

        /// <summary>Lo saca del sistema: cierra todas sus sesiones abiertas. Si ya estaba inactivo, no cambia nada.</summary>
        /// <param name="openSessions">Los refresh tokens del usuario que todavía no se anularon.</param>
        public void Deactivate(UserRole actorRole, IReadOnlyCollection<RefreshToken> openSessions, DateTime now)
        {
            DomainException.ThrowIf(ManageError(actorRole));
            IsActive = false;
            CloseSessions(openSessions, now);
        }

        private void CloseSessions(IReadOnlyCollection<RefreshToken> openSessions, DateTime now)
        {
            if (openSessions.Any(t => t.UserId != Id))
                throw new DomainException("Las sesiones no son de este usuario.");

            foreach (var token in openSessions)
                token.Revoke(now);
        }

        /// <summary>El aviso de "correo ya usado", el mismo al crear o editar un usuario: dice de quién es.</summary>
        /// <param name="owner">El usuario que ya tiene ese correo.</param>
        public static string EmailTakenError(User owner) =>
            $"El usuario {owner.Name}{(owner.IsActive ? "" : " (desactivado)")} ya tiene el correo {owner.Email}. Usa otro correo.";

        /// <summary>Qué impide a quien tiene ese rol modificar a este usuario, o null si puede.</summary>
        public string? ManageError(UserRole actorRole) =>
            actorRole.CanManage(Role) ? null : "No tienes permisos sobre este usuario.";

        /// <summary>Qué impide a quien tiene ese rol dar el rol indicado, o null si puede (solo roles menores al suyo).</summary>
        public static string? AssignRoleError(UserRole actorRole, UserRole role) =>
            actorRole.CanManage(role) ? null : "No tienes permisos para asignar este rol.";

        /// <summary>El nombre tal como se guarda: sin espacios al inicio ni al final, ni dobles en medio.</summary>
        public static string NormalizeName(string name) =>
            TextNormalizer.CollapseSpaces(name);

        /// <summary>Qué tiene de malo el nombre, o null si está bien.</summary>
        public static string? NameError(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "El nombre es requerido.";

            if (NormalizeName(name).Length > NameMaxLength)
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

            // MailAddress también acepta "Juan <juan@empresa.pe>": se exige que lo escrito sea solo la dirección.
            if (!MailAddress.TryCreate(normalized, out var parsed) || parsed.Address != normalized)
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
            DomainException.ThrowIf(NameError(name));
            DomainException.ThrowIf(EmailError(email));
            DomainException.ThrowIf(PasswordError(password));
            DomainException.ThrowIf(RoleError(role));

            return new(Guid.CreateVersion7(), NormalizeName(name), NormalizeEmail(email), hashPassword(password), role, isActive: true);
        }
    }
}
