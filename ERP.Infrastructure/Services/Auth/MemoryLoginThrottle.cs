using System.Collections.Concurrent;
using ERP.Application.Contracts.Infrastructure;

namespace ERP.Infrastructure.Services.Auth
{
    /// <summary>
    /// Cuenta los intentos fallidos en memoria, como el "login cooldown" de Odoo: 5 fallos seguidos con el mismo correo
    /// o desde el mismo equipo obligan a esperar 1 minuto desde el último. Se olvida al reiniciar la API, lo que basta
    /// con un solo servidor; con varios, iría en una caché compartida.
    /// </summary>
    internal sealed class MemoryLoginThrottle : ILoginThrottle
    {
        private const int MaxFailures = 5;
        private static readonly TimeSpan Cooldown = TimeSpan.FromMinutes(1);
        // Para no crecer sin límite: se limpian las entradas viejas de vez en cuando.
        private const int CleanupThreshold = 1000;

        private readonly ConcurrentDictionary<string, Failures> _failures = new();
        private readonly TimeProvider _timeProvider;

        public MemoryLoginThrottle(TimeProvider timeProvider) => _timeProvider = timeProvider;

        private sealed record Failures(int Count, DateTimeOffset Last);

        public TimeSpan? WaitFor(string email, string? clientIp)
        {
            var now = _timeProvider.GetUtcNow();
            var wait = Keys(email, clientIp)
                .Select(key => _failures.TryGetValue(key, out var f) && f.Count >= MaxFailures ? f.Last + Cooldown - now : TimeSpan.Zero)
                .Max();

            return wait > TimeSpan.Zero ? wait : null;
        }

        public void RecordFailure(string email, string? clientIp)
        {
            var now = _timeProvider.GetUtcNow();

            foreach (var key in Keys(email, clientIp))
                // Un fallo después de un minuto sin fallos empieza la cuenta de nuevo.
                _failures.AddOrUpdate(key, new Failures(1, now), (_, f) => now - f.Last > Cooldown ? new Failures(1, now) : new Failures(f.Count + 1, now));

            if (_failures.Count > CleanupThreshold)
                foreach (var (key, f) in _failures)
                    if (now - f.Last > Cooldown)
                        _failures.TryRemove(key, out _);
        }

        public void Reset(string email, string? clientIp)
        {
            foreach (var key in Keys(email, clientIp))
                _failures.TryRemove(key, out _);
        }

        // El correo se compara como se guarda (sin espacios y en minúsculas).
        private static IEnumerable<string> Keys(string email, string? clientIp)
        {
            yield return "email:" + email.Trim().ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(clientIp))
                yield return "ip:" + clientIp;
        }
    }
}
