namespace ERP.Application.Contracts.Infrastructure
{
    /// <summary>
    /// Cuenta las contraseñas equivocadas al iniciar sesión, por correo y por equipo (IP), como el "login cooldown" de
    /// Odoo: después de varios fallos seguidos hay que esperar un momento. No bloquea la cuenta: solo hace esperar.
    /// </summary>
    public interface ILoginThrottle
    {
        /// <summary>Cuánto falta para poder intentar de nuevo, o null si se puede intentar ya.</summary>
        TimeSpan? WaitFor(string email, string? clientIp);

        void RecordFailure(string email, string? clientIp);

        /// <summary>Al entrar bien, se olvidan los fallos de ese correo y ese equipo.</summary>
        void Reset(string email, string? clientIp);
    }
}
