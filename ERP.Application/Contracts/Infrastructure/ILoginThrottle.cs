namespace ERP.Application.Contracts.Infrastructure
{
    /// <summary>
    /// Limita los intentos de inicio de sesión, como el "login cooldown" de Odoo: después de varios fallos seguidos hay
    /// que esperar un momento. No bloquea la cuenta: solo hace esperar.
    /// </summary>
    public interface ILoginThrottle
    {
        /// <summary>
        /// Reserva un intento antes de revisar la contraseña: cuenta como fallo hasta que se confirme que entró bien
        /// (<see cref="Succeeded"/>). Así varios intentos enviados a la vez no se saltan el límite.
        /// </summary>
        /// <returns>Cuánto falta para poder intentar de nuevo (y no se reserva nada), o null si se puede intentar ya.</returns>
        TimeSpan? TryBegin(string email, string? clientIp);

        /// <summary>Entró bien: se olvidan los fallos de ese correo desde ese equipo.</summary>
        void Succeeded(string email, string? clientIp);
    }
}
