using ERP.Domain.Common;

namespace ERP.Domain.Users
{
    /// <summary>
    /// Una sesión iniciada en un navegador. El navegador guarda el token en una cookie que JavaScript no puede leer; aquí
    /// solo se guarda su huella (hash), así que quien lea la base no puede usarlo. Cada renovación entrega un token nuevo y
    /// anula el anterior (rotación). Todos los tokens de una misma sesión comparten <see cref="FamilyId"/>: si se presenta
    /// uno ya anulado, alguien lo copió, y se anula la sesión entera.
    /// </summary>
    public sealed class RefreshToken : BaseEntity
    {
        /// <summary>La sesión vence tras este tiempo sin usar el sistema (decisión 23).</summary>
        public static readonly TimeSpan IdleLifetime = TimeSpan.FromHours(8);

        /// <summary>Tope desde que se inició sesión, aunque se use todos los días (como los 7 días de Odoo).</summary>
        public static readonly TimeSpan AbsoluteLifetime = TimeSpan.FromDays(7);

        /// <summary>
        /// Dos pestañas pueden renovar a la vez con el mismo token: si el anulado llega dentro de este margen, no es un robo
        /// (como el "reuse interval" de Auth0).
        /// </summary>
        public static readonly TimeSpan ReuseInterval = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Un token vencido se guarda este tiempo más: si alguien presenta uno copiado, todavía se reconoce como robo.
        /// Después se puede borrar.
        /// </summary>
        public static readonly TimeSpan KeepAfterExpiry = TimeSpan.FromDays(1);

        /// <summary>Largo de la huella: SHA-256 en hexadecimal.</summary>
        public const int HashLength = 64;

        /// <summary>El aviso cuando la sesión ya no sirve (vencida, anulada o desconocida): uno solo en el dominio y en la renovación.</summary>
        public const string ExpiredError = "Tu sesión venció. Vuelve a iniciar sesión.";

        public Guid UserId { get; }
        public string TokenHash { get; }
        public Guid FamilyId { get; }
        public DateTime CreatedAt { get; }
        /// <summary>Vence si no se renueva antes: 8 horas después de creado, sin pasar el tope de la sesión.</summary>
        public DateTime ExpiresAt { get; }
        /// <summary>Tope de la sesión entera: no cambia al renovar.</summary>
        public DateTime SessionExpiresAt { get; }
        public DateTime? RevokedAt { get; private set; }
        /// <summary>El token que lo reemplazó al renovar, o null si se anuló sin reemplazo (cierre de sesión, robo).</summary>
        public Guid? ReplacedById { get; private set; }

        private RefreshToken(Guid id, Guid userId, string tokenHash, Guid familyId, DateTime createdAt, DateTime expiresAt, DateTime sessionExpiresAt) : base(id)
        {
            UserId = userId;
            TokenHash = tokenHash;
            FamilyId = familyId;
            CreatedAt = createdAt;
            ExpiresAt = expiresAt;
            SessionExpiresAt = sessionExpiresAt;
        }

        /// <summary>El primer token de una sesión, al iniciar sesión.</summary>
        public static RefreshToken Start(Guid userId, string tokenHash, DateTime now)
        {
            DomainException.ThrowIf(userId == Guid.Empty ? "El usuario es requerido." : null);
            DomainException.ThrowIf(HashError(tokenHash));

            var sessionExpiresAt = now + AbsoluteLifetime;
            return new(Guid.CreateVersion7(), userId, tokenHash, Guid.CreateVersion7(), now, Min(now + IdleLifetime, sessionExpiresAt), sessionExpiresAt);
        }

        /// <summary>Renueva: anula este token y devuelve el siguiente de la misma sesión.</summary>
        public RefreshToken Rotate(string newTokenHash, DateTime now)
        {
            DomainException.ThrowIf(RefreshError(now));
            DomainException.ThrowIf(HashError(newTokenHash));

            var next = new RefreshToken(Guid.CreateVersion7(), UserId, newTokenHash, FamilyId, now, Min(now + IdleLifetime, SessionExpiresAt), SessionExpiresAt);
            RevokedAt = now;
            ReplacedById = next.Id;
            return next;
        }

        /// <summary>Anula el token (cerrar sesión, usuario desactivado, contraseña restablecida). Si ya estaba anulado, no cambia nada.</summary>
        public void Revoke(DateTime now) =>
            RevokedAt ??= now;

        public bool IsRevoked => RevokedAt is not null;

        /// <summary>Si todavía sirve para renovar: no está anulado ni vencido. Una sesión está abierta mientras tenga uno así.</summary>
        public bool IsActive(DateTime now) =>
            RefreshError(now) is null;

        /// <summary>
        /// Si este token, ya reemplazado, se presentó dentro del margen de las pestañas (o del reintento tras una respuesta
        /// que no llegó): entonces no es un robo.
        /// </summary>
        public bool IsWithinReuseInterval(DateTime now) =>
            RevokedAt is { } revokedAt && ReplacedById is not null && now - revokedAt <= ReuseInterval;

        /// <summary>Qué impide renovar con este token, o null si se puede.</summary>
        public string? RefreshError(DateTime now) =>
            IsRevoked || now >= ExpiresAt ? ExpiredError : null;

        /// <summary>
        /// Si se presentó un token ya reemplazado fuera del margen de las pestañas: alguien lo copió y hay que anular
        /// la sesión entera.
        /// </summary>
        public bool IsReuse(DateTime now) =>
            IsRevoked && !IsWithinReuseInterval(now);

        // La huella es un SHA-256 en hexadecimal: 64 caracteres (lo mismo que guarda la columna).
        private static string? HashError(string tokenHash) =>
            string.IsNullOrWhiteSpace(tokenHash) || tokenHash.Length != HashLength || !tokenHash.All(char.IsAsciiHexDigit)
                ? "La huella del token debe ser un SHA-256 en hexadecimal."
                : null;

        private static DateTime Min(DateTime a, DateTime b) => a < b ? a : b;
    }
}
