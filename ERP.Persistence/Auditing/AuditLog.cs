using ERP.Application.Features.Audit;

namespace ERP.Persistence.Auditing
{
    /// <summary>
    /// Un evento del historial. Lo escribe <see cref="Interceptors.AuditInterceptor"/> en la misma transacción
    /// que el cambio: si el cambio no se guarda, el evento tampoco. No se modifica ni se borra.
    /// </summary>
    internal sealed class AuditLog
    {
        public const int EntityLabelMaxLength = 250;

        public Guid Id { get; private set; }
        public DateTime OccurredAt { get; private set; }
        public Guid UserId { get; private set; }
        public AuditEntityType EntityType { get; private set; }
        public Guid EntityId { get; private set; }
        public string EntityLabel { get; private set; }
        public AuditAction Action { get; private set; }
        public List<AuditLogChange> Changes { get; private set; }

        // Para EF Core al leer.
        private AuditLog()
        {
            EntityLabel = null!;
            Changes = null!;
        }

        public AuditLog(DateTime occurredAt, Guid userId, AuditEntityType entityType, Guid entityId, string entityLabel, AuditAction action, List<AuditLogChange> changes)
        {
            Id = Guid.CreateVersion7();
            OccurredAt = occurredAt;
            UserId = userId;
            EntityType = entityType;
            EntityId = entityId;
            EntityLabel = entityLabel.Length > EntityLabelMaxLength ? entityLabel[..EntityLabelMaxLength] : entityLabel;
            Action = action;
            Changes = changes;
        }
    }
}
