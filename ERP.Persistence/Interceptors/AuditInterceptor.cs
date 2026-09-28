using ERP.Application.Contracts.Api;
using ERP.Application.Features.Audit;
using ERP.Domain.Common;
using ERP.Persistence.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ERP.Persistence.Interceptors
{
    /// <summary>
    /// Antes de guardar: completa quién creó o modificó cada registro y escribe su historial en AuditLogs.
    /// Todo va en la misma transacción que el cambio, así ningún cambio queda sin registrar, venga de donde venga
    /// (pantallas, carga masiva o el propio sistema).
    /// </summary>
    internal sealed class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly ICurrentUser _currentUser;

        public AuditInterceptor(ICurrentUser currentUser) => _currentUser = currentUser;

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken ct = default)
        {
            if(eventData.Context is { } context)
            {
                var now = DateTime.UtcNow;
                var userId = _currentUser.IsAuthenticated ? _currentUser.Id : AuditableEntity.SystemUserId;
                var logs = new List<AuditLog>();

                foreach(var entry in context.ChangeTracker.Entries<AuditableEntity>())
                {
                    if (entry.State == EntityState.Added)
                    {
                        entry.Property(e => e.CreatedAt).CurrentValue = now;
                        entry.Property(e => e.CreatedBy).CurrentValue = userId;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        entry.Property(e => e.UpdatedAt).CurrentValue = now;
                        entry.Property(e => e.UpdatedBy).CurrentValue = userId;
                    }

                    if (ToAuditLog(entry, now, userId) is { } log)
                        logs.Add(log);
                }

                context.AddRange(logs);
            }

            return base.SavingChangesAsync(eventData, result, ct);
        }

        private static AuditLog? ToAuditLog(EntityEntry<AuditableEntity> entry, DateTime now, Guid userId)
        {
            if (AuditDescriber.Describe(entry.Entity) is not { } subject)
                return null;

            if (entry.State == EntityState.Added)
                return new AuditLog(now, userId, subject.Type, entry.Entity.Id, subject.Label, AuditAction.Created, []);

            if (entry.State != EntityState.Modified)
                return null;

            // Solo cuentan los campos que el usuario cambia; UpdatedAt, UpdatedBy y la versión quedan fuera.
            var modified = entry.Properties
                .Where(p => p.IsModified && !Equals(p.OriginalValue, p.CurrentValue))
                .Where(p => p.Metadata.Name == AuditDescriber.PasswordProperty || AuditDescriber.IsAudited(subject.Type, p.Metadata.Name))
                .ToDictionary(p => p.Metadata.Name);

            if (modified.Count == 0)
                return null;

            // La contraseña es una acción propia y nunca se guarda su valor.
            if (modified.ContainsKey(AuditDescriber.PasswordProperty))
                return new AuditLog(now, userId, subject.Type, entry.Entity.Id, subject.Label, AuditAction.PasswordReset, []);

            var action = ActionFor(modified);

            // En activar, desactivar y anular, la acción ya dice el cambio de estado; el resto de campos
            // (como el motivo de anulación) sí se muestran.
            var changes = modified.Values
                .Where(p => action == AuditAction.Updated || !AuditDescriber.IsStatus(p.Metadata.Name))
                .Select(p => AuditDescriber.Change(subject.Type, p.Metadata.Name, p.OriginalValue, p.CurrentValue))
                .Select(c => new AuditLogChange(c.Field, c.From, c.To))
                .ToList();

            return new AuditLog(now, userId, subject.Type, entry.Entity.Id, subject.Label, action, changes);
        }

        private static AuditAction ActionFor(IReadOnlyDictionary<string, PropertyEntry> modified)
        {
            if (modified.TryGetValue(AuditDescriber.IsCancelledProperty, out var cancelled) && cancelled.CurrentValue is true)
                return AuditAction.Cancelled;

            if (modified.Count == 1 && modified.TryGetValue(AuditDescriber.IsActiveProperty, out var active))
                return active.CurrentValue is true ? AuditAction.Activated : AuditAction.Deactivated;

            return AuditAction.Updated;
        }
    }
}
