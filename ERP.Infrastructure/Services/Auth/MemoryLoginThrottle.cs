using ERP.Application.Contracts.Infrastructure;
using ERP.Domain.Users;

namespace ERP.Infrastructure.Services.Auth
{
    /// <summary>
    /// Cuenta los intentos fallidos en memoria, como el "login cooldown" de Odoo. Hay tres cuentas, y si cualquiera llega
    /// a su límite hay que esperar 1 minuto desde el último fallo:
    /// - el mismo correo desde el mismo equipo (IP): 5 fallos. Es el caso normal de alguien que olvidó su contraseña.
    /// - el mismo correo desde cualquier equipo: 20. Así un tercero no deja a alguien sin poder entrar con solo 5 intentos.
    /// - el mismo equipo con cualquier correo: 20. Frena a quien prueba muchas cuentas desde un equipo.
    /// Se olvida al reiniciar la API, lo que basta con un solo servidor; con varios, iría en una caché compartida.
    /// </summary>
    internal sealed class MemoryLoginThrottle : ILoginThrottle
    {
        private const int PairMaxFailures = 5;
        private const int WideMaxFailures = 20;
        private static readonly TimeSpan Cooldown = TimeSpan.FromMinutes(1);
        // Para no crecer sin límite: se limpian las entradas viejas de vez en cuando.
        private const int CleanupThreshold = 1000;

        // Un candado basta: son pocos inicios de sesión, y así revisar y reservar el intento es una sola operación.
        private readonly Lock _gate = new();
        private readonly Dictionary<string, Failures> _failures = [];
        private readonly TimeProvider _timeProvider;

        public MemoryLoginThrottle(TimeProvider timeProvider) => _timeProvider = timeProvider;

        private sealed record Failures(int Count, DateTimeOffset Last);

        public TimeSpan? TryBegin(string email, string? clientIp)
        {
            var now = _timeProvider.GetUtcNow();
            var keys = Keys(email, clientIp);

            lock (_gate)
            {
                // Mientras hay que esperar, los intentos no cuentan ni alargan la espera: si no, un tercero podría dejar
                // a alguien sin entrar indefinidamente.
                var wait = keys
                    .Select(k => _failures.TryGetValue(k.Key, out var f) && f.Count >= k.Max ? f.Last + Cooldown - now : TimeSpan.Zero)
                    .Max();
                if (wait > TimeSpan.Zero)
                    return wait;

                // Un fallo después de un minuto sin fallos empieza la cuenta de nuevo.
                foreach (var (key, _) in keys)
                    _failures[key] = _failures.TryGetValue(key, out var f) && now - f.Last <= Cooldown
                        ? new Failures(f.Count + 1, now)
                        : new Failures(1, now);

                if (_failures.Count > CleanupThreshold)
                    foreach (var (key, f) in _failures.ToList())
                        if (now - f.Last > Cooldown)
                            _failures.Remove(key);

                return null;
            }
        }

        public void Succeeded(string email, string? clientIp)
        {
            lock (_gate)
            {
                foreach (var (key, max) in Keys(email, clientIp))
                {
                    // El par correo y equipo se olvida; en las cuentas amplias solo se descuenta este intento, que no
                    // fue un fallo (entrar bien no borra lo que otro probó con otros correos desde la misma red).
                    if (max == PairMaxFailures || !_failures.TryGetValue(key, out var f) || f.Count <= 1)
                        _failures.Remove(key);
                    else
                        _failures[key] = f with { Count = f.Count - 1 };
                }
            }
        }

        // El correo se compara como se guarda (sin espacios y en minúsculas).
        private static List<(string Key, int Max)> Keys(string email, string? clientIp)
        {
            var normalized = User.NormalizeEmail(email);
            var keys = new List<(string Key, int Max)> { ("email:" + normalized, WideMaxFailures) };
            if (!string.IsNullOrWhiteSpace(clientIp))
            {
                keys.Add(("pair:" + normalized + "|" + clientIp, PairMaxFailures));
                keys.Add(("ip:" + clientIp, WideMaxFailures));
            }
            else
                // Sin IP (no debería pasar), el correo solo lleva el límite estricto.
                keys[0] = ("email:" + normalized, PairMaxFailures);

            return keys;
        }
    }
}
